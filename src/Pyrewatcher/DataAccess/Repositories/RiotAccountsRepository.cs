using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class RiotAccountsRepository : RepositoryBase, IRiotAccountsRepository
  {
    public RiotAccountsRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<RiotAccount> GetChannelAccountForLookupByKeyAsync(long channelId, string accountKey)
    {
      const string query = @"SELECT TOP 1 [RA].[ServerStr], [RAG].[GameStr], [RA].[SummonerName], [CRAG].[DisplayName],
  [CRAG].[Active], [RAG].[Tier], [RAG].[Rank], [RAG].[LeaguePoints], [RAG].[SeriesProgress]
FROM [ChannelRiotAccountGames] [CRAG]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<RiotAccount>(query, new { channelId, accountKey });

      if (result is not null)
      {
        result.Server = Enum.Parse<Server>(result.ServerStr);
        result.Game = Enum.Parse<Game>(result.GameStr);
      }

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveAccountsForDisplayByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [CRAG].[Key], [CRAG].[DisplayName]
FROM [ChannelRiotAccountGames] [CRAG]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Active] = 1
ORDER BY [CRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { channelId });

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetInactiveAccountsForDisplayByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [CRAG].[Key], [CRAG].[DisplayName]
FROM [ChannelRiotAccountGames] [CRAG]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Active] = 0
ORDER BY [CRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { channelId });

      return result;
    }

    public async Task<bool> UnassignAccountGameFromChannelByKeyAsync(string accountKey)
    {
      const string query = @"DELETE FROM [ChannelRiotAccountGames]
WHERE [Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new {accountKey});

      return rows == 1;
    }

    public async Task<bool> InsertAccountGameFromDtoAsync(Game game, Server server, ISummonerDto summoner)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summoner.SummonerName);

      const string query = @"DECLARE @RiotAccountId BIGINT = NULL;

SELECT TOP 1 @RiotAccountId = [RA].[Id]
FROM [RiotAccounts] [RA]
WHERE [RA].[ServerStr] = @serverStr AND [RA].[NormalizedSummonerName] = @normalizedSummonerName;

IF @RiotAccountId IS NULL
BEGIN
  DECLARE @IdTable TABLE ([Id] BIGINT);

  INSERT INTO [RiotAccounts] ([ServerStr], [SummonerName], [NormalizedSummonerName])
  OUTPUT [inserted].[Id] INTO @IdTable
  VALUES (@serverStr, @summonerName, @normalizedSummonerName);

  SELECT TOP 1 @RiotAccountId = [Id]
  FROM @IdTable;
END

INSERT INTO [RiotAccountGames] ([RiotAccountId], [GameStr], [SummonerId], [AccountId], [Puuid])
VALUES (@RiotAccountId, @gameStr, @summonerId, @accountId, @puuid);";

      var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        serverStr,
        normalizedSummonerName,
        summonerName = summoner.SummonerName,
        gameStr,
        summonerId = summoner.SummonerId,
        accountId = summoner.AccountId,
        puuid = summoner.Puuid
      });

      return rows is 1 or 2;
    }

    public async Task<bool> ToggleActiveByKeyAsync(string accountKey)
    {
      const string query = @"UPDATE [ChannelRiotAccountGames]
SET [Active] = ~[Active]
WHERE [Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey });

      return rows == 1;
    }

    public async Task<bool> UpdateDisplayNameByKeyAsync(string accountKey, string displayName)
    {
      const string query = @"UPDATE [ChannelRiotAccountGames]
SET [DisplayName] = @displayName
WHERE [Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, displayName });

      return rows == 1;
    }

    public async Task<bool> UpdateSummonerNameByKeyAsync(string accountKey, string summonerName)
    {
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"UPDATE [RA]
SET [RA].[SummonerName] = @summonerName, [RA].[NormalizedSummonerName] = @normalizedSummonerName
FROM [RiotAccounts] [RA]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[RiotAccountId] = [RA].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [RAG].[Id]
WHERE [CRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, summonerName, normalizedSummonerName });

      return rows == 1;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [CRAG].[Key], [RA].[SummonerName], [RA].[ServerStr],
  [CRAG].[DisplayName], [RAG].[SummonerId], [RAG].[AccountId], [RAG].[Puuid],
  [RAG].[Tier], [RAG].[Rank], [RAG].[LeaguePoints], [RAG].[SeriesProgress]
FROM [ChannelRiotAccountGames] [CRAG]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Active] = 1 AND [RAG].[GameStr] = 'LeagueOfLegends';";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new {channelId});
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
      }

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveAccountsWithRankByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [RA].[ServerStr], [RAG].[GameStr], [RA].[SummonerName], [CRAG].[DisplayName],
  [RAG].[Tier], [RAG].[Rank], [RAG].[LeaguePoints], [RAG].[SeriesProgress]
FROM [ChannelRiotAccountGames] [CRAG]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Active] = 1
ORDER BY [CRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { channelId });
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
        account.Game = Enum.Parse<Game>(account.GameStr);
      }

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveTftAccountsForApiCallsByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [CRAG].[Key], [RA].[SummonerName], [RA].[ServerStr],
  [CRAG].[DisplayName], [RAG].[SummonerId], [RAG].[AccountId], [RAG].[Puuid],
  [RAG].[Tier], [RAG].[Rank], [RAG].[LeaguePoints], [RAG].[SeriesProgress]
FROM [ChannelRiotAccountGames] [CRAG]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [CRAG].[ChannelId] = @broadcasterId AND [CRAG].[Active] = 1 AND [RAG].[GameStr] = 'TeamfightTactics';";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new {broadcasterId = channelId });
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
      }

      return result;
    }

    public async Task<bool> UpdateRankByKeyAsync(string accountKey, string tier, string rank, string leaguePoints, string seriesProgress)
    {
      const string query = @"UPDATE [RAG]
SET [RAG].[Tier] = @tier, [RAG].[Rank] = @rank, [RAG].[LeaguePoints] = @leaguePoints, [RAG].[SeriesProgress] = @seriesProgress
FROM [RiotAccountGames] [RAG]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [RAG].[Id]
WHERE [CRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, tier, rank, leaguePoints, seriesProgress });

      return rows == 1;
    }

    public async Task<bool> IsAccountGameAssignedToChannelAsync(long channelId, Game game, Server server, string summonerName)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT TOP 1 *
  FROM [ChannelRiotAccountGames] [CRAG]
  INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
  INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
  WHERE [CRAG].[ChannelId] = @channelId AND [RA].[ServerStr] = @serverStr
    AND [RAG].[GameStr] = @gameStr AND [RA].[NormalizedSummonerName] = @normalizedSummonerName
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleAsync<bool>(query, new {channelId, gameStr, serverStr, normalizedSummonerName});

      return result;
    }

    public async Task<bool> ExistsAccountGameAsync(Game game, Server server, string summonerName)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT TOP 1 *
  FROM [RiotAccountGames] [RAG]
  INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
  WHERE [RA].[ServerStr] = @serverStr AND [RAG].[GameStr] = @gameStr
    AND [RA].[NormalizedSummonerName] = @normalizedSummonerName
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleAsync<bool>(query, new { gameStr, serverStr, normalizedSummonerName });

      return result;
    }

    public async Task<bool> AssignAccountGameToChannelAsync(long channelId, Game game, Server server, string summonerName, string accountKey)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"DECLARE @RiotAccountGameId BIGINT = NULL;

SELECT TOP 1 @RiotAccountGameId = [RAG].[Id]
FROM [RiotAccountGames] [RAG]
INNER JOIN [RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [RAG].[GameStr] = @gameStr AND [RA].[ServerStr] = @serverStr AND [RA].[NormalizedSummonerName] = @normalizedSummonerName;

INSERT INTO [ChannelRiotAccountGames] ([ChannelId], [RiotAccountGameId], [Key], [DisplayName], [Active])
VALUES (@channelId, @RiotAccountGameId, @accountKey, @displayName, 1);";

      var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        gameStr,
        serverStr,
        normalizedSummonerName,
        channelId,
        accountKey,
        displayName = $"{game.ToAbbreviation()} {server} {summonerName}"
      });

      return rows == 1;
    }

    public async Task<string> GetAccountSummonerNameAsync(Server server, string normalizedSummonerName)
    {
      var serverStr = server.ToString();

      const string query = @"SELECT TOP 1 [SummonerName]
FROM [RiotAccounts]
WHERE [ServerStr] = @serverStr AND [NormalizedSummonerName] = @normalizedSummonerName;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<string>(query, new {serverStr, normalizedSummonerName});

      return result;
    }

    public async Task<bool> InsertRankChangeByKeyAsync(string accountKey, string oldTier, string oldRank, string oldLeaguePoints,
                                                           string oldSeriesProgress, string newTier, string newRank, string newLeaguePoints,
                                                           string newSeriesProgress)
    {
      var timestamp = DateTime.UtcNow;

      const string query = @"INSERT INTO [RiotAccountRankChanges] ([RiotAccountGameId], [Timestamp], [OldTier], [OldRank],
  [OldLeaguePoints], [OldSeriesProgress], [NewTier], [NewRank], [NewLeaguePoints], [NewSeriesProgress])
VALUES ((SELECT [RiotAccountGameId] FROM [ChannelRiotAccountGames] WHERE [Key] = @accountKey), @timestamp, @oldTier,
  @oldRank, @oldLeaguePoints, @oldSeriesProgress, @newTier, @newRank, @newLeaguePoints, @newSeriesProgress);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        accountKey,
        timestamp,
        oldTier,
        oldRank,
        oldLeaguePoints,
        oldSeriesProgress,
        newTier,
        newRank,
        newLeaguePoints,
        newSeriesProgress
      });

      return rows == 1;
    }

    public async Task<IEnumerable<RankChange>> GetTodaysRankChangesByChannelIdAsync(long channelId)
    {
      var timestamp = RiotUtilities.GetStartTime();

      const string query = @"SELECT [CRAG].[DisplayName], [RARC].[Timestamp],
  [RARC].[OldTier], [RARC].[OldRank], [RARC].[OldLeaguePoints], [RARC].[OldSeriesProgress],
  [RARC].[NewTier], [RARC].[NewRank], [RARC].[NewLeaguePoints], [RARC].[NewSeriesProgress]
FROM [ChannelRiotAccountGames] [CRAG]
INNER JOIN [RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [RiotAccountRankChanges] [RARC] ON [RARC].[RiotAccountGameId] = [RAG].[Id]
WHERE [RARC].[Timestamp] >= @timestamp AND [RARC].[OldTier] IS NOT NULL AND [RARC].[NewTier] IS NOT NULL
  AND [CRAG].[Active] = 1 AND [CRAG].[ChannelId] = @channelId
ORDER BY [RARC].[Timestamp];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RankChange>(query, new {timestamp, channelId});

      return result;
    }
  }
}
