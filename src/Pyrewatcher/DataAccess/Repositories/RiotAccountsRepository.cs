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

    public async Task<NewRiotAccount> NewGetChannelAccountForLookupByKeyAsync(long channelId, string accountKey)
    {
      const string query = @"SELECT TOP 1 [NRA].[ServerStr], [NRAG].[GameStr], [NRA].[SummonerName], [NCRAG].[DisplayName],
  [NCRAG].[Active], [NRAG].[Tier], [NRAG].[Rank], [NRAG].[LeaguePoints], [NRAG].[SeriesProgress]
FROM [NewChannelRiotAccountGames] [NCRAG]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
WHERE [NCRAG].[ChannelId] = @channelId AND [NCRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<NewRiotAccount>(query, new { channelId, accountKey });

      if (result is not null)
      {
        result.Server = Enum.Parse<Server>(result.ServerStr);
        result.Game = Enum.Parse<Game>(result.GameStr);
      }

      return result;
    }

    public async Task<IEnumerable<NewRiotAccount>> NewGetActiveAccountsForDisplayByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [NCRAG].[Key], [NCRAG].[DisplayName]
FROM [NewChannelRiotAccountGames] [NCRAG]
WHERE [NCRAG].[ChannelId] = @channelId AND [NCRAG].[Active] = 1
ORDER BY [NCRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewRiotAccount>(query, new { channelId });

      return result;
    }

    public async Task<IEnumerable<NewRiotAccount>> NewGetInactiveAccountsForDisplayByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [NCRAG].[Key], [NCRAG].[DisplayName]
FROM [NewChannelRiotAccountGames] [NCRAG]
WHERE [NCRAG].[ChannelId] = @channelId AND [NCRAG].[Active] = 0
ORDER BY [NCRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewRiotAccount>(query, new { channelId });

      return result;
    }

    public async Task<bool> NewDeleteChannelAccountByKey(string accountKey)
    {
      const string query = @"DECLARE @RiotAccountGameId BIGINT;
DECLARE @RiotAccountId BIGINT;

SELECT @RiotAccountId = [NRAG].[Id], @RiotAccountGameId = [NCRAG].[RiotAccountGameId]
FROM [NewChannelRiotAccountGames] [NCRAG]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
WHERE [NCRAG].[Key] = @accountKey;

DELETE FROM [NewChannelRiotAccountGames]
WHERE [Key] = @accountKey;

IF NOT EXISTS (
  SELECT TOP 1 *
  FROM [NewChannelRiotAccountGames]
  WHERE [RiotAccountGameId] = @RiotAccountGameId
)
  DELETE FROM [NewRiotAccountGames]
  WHERE [Id] = @RiotAccountGameId;

IF NOT EXISTS (
  SELECT TOP 1 *
  FROM [NewRiotAccountGames]
  WHERE [RiotAccountId] = @RiotAccountId
)
  DELETE FROM [NewRiotAccounts]
  WHERE [Id] = @RiotAccountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new {accountKey});

      return rows > 0;
    }

    public async Task<bool> NewInsertAccountGameFromDto(Game game, Server server, ISummonerDto summoner)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summoner.SummonerName);

      const string query = @"DECLARE @RiotAccountId BIGINT = NULL;

SELECT TOP 1 @RiotAccountId = [NRA].[Id]
FROM [NewRiotAccounts] [NRA]
WHERE [NRA].[ServerStr] = @serverStr AND [NRA].[NormalizedSummonerName] = @normalizedSummonerName;

IF @RiotAccountId IS NULL
BEGIN
  DECLARE @IdTable TABLE ([Id] BIGINT);

  INSERT INTO [NewRiotAccounts] ([ServerStr], [SummonerName], [NormalizedSummonerName])
  OUTPUT [inserted].[Id] INTO @IdTable
  VALUES (@serverStr, @summonerName, @normalizedSummonerName);

  SELECT TOP 1 @RiotAccountId = [Id]
  FROM @IdTable;
END

INSERT INTO [NewRiotAccountGames] ([RiotAccountId], [GameStr], [SummonerId], [AccountId], [Puuid])
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

    public async Task<bool> NewToggleActiveByKey(string accountKey)
    {
      const string query = @"UPDATE [NewRiotAccounts]
SET [Active] = ~[Active]
WHERE [Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey });

      return rows == 1;
    }

    public async Task<bool> NewUpdateDisplayNameByKeyAsync(string accountKey, string displayName)
    {
      const string query = @"UPDATE [NewRiotAccounts]
SET [DisplayName] = @displayName
WHERE [Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, displayName });

      return rows == 1;
    }

    public async Task<bool> NewUpdateSummonerNameByKeyAsync(string accountKey, string summonerName)
    {
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"UPDATE [NRA]
SET [NRA].[SummonerName] = @summonerName, [NRA].[NormalizedSummonerName] = @normalizedSummonerName
FROM [NewRiotAccounts] [NRA]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[RiotAccountId] = [NRA].[Id]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NRAG].[Id]
WHERE [NCRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, summonerName, normalizedSummonerName });

      return rows == 1;
    }

    public async Task<IEnumerable<NewRiotAccount>> NewGetActiveLolAccountsForApiCallsByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [NCRAG].[Key], [NRA].[SummonerName], [NRA].[ServerStr],
  [NCRAG].[DisplayName], [NRAG].[SummonerId], [NRAG].[AccountId], [NRAG].[Puuid]
FROM [NewChannelRiotAccountGames] [NCRAG]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
WHERE [NCRAG].[ChannelId] = @broadcasterId AND [NCRAG].[Active] = 1 AND [NRAG].[GameStr] = 'LeagueOfLegends';";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewRiotAccount>(query, new {broadcasterId = channelId});
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
      }

      return result;
    }

    public async Task<IEnumerable<NewRiotAccount>> NewGetActiveAccountsWithRankByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [NRA].[ServerStr], [NRAG].[GameStr], [NRA].[SummonerName], [NCRAG].[DisplayName],
  [NRAG].[Tier], [NRAG].[Rank], [NRAG].[LeaguePoints], [NRAG].[SeriesProgress]
FROM [NewChannelRiotAccountGames] [NCRAG]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
WHERE [NCRAG].[ChannelId] = @channelId AND [NCRAG].[Active] = 1
ORDER BY [NCRAG].[DisplayName];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewRiotAccount>(query, new { channelId });
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
        account.Game = Enum.Parse<Game>(account.GameStr);
      }

      return result;
    }

    public async Task<IEnumerable<NewRiotAccount>> NewGetActiveTftAccountsForApiCallsByChannelIdAsync(long channelId)
    {
      const string query = @"SELECT [NCRAG].[Key], [NRA].[SummonerName], [NRA].[ServerStr],
  [NCRAG].[DisplayName], [NRAG].[SummonerId], [NRAG].[AccountId], [NRAG].[Puuid]
FROM [NewChannelRiotAccountGames] [NCRAG]
INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
WHERE [NCRAG].[ChannelId] = @broadcasterId AND [NCRAG].[Active] = 1 AND [NRAG].[GameStr] = 'TeamfightTactics';";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewRiotAccount>(query, new {broadcasterId = channelId });
      foreach (var account in result)
      {
        account.Server = Enum.Parse<Server>(account.ServerStr);
      }

      return result;
    }

    public async Task<bool> NewUpdateRankByKeyAsync(string accountKey, string tier, string rank, string leaguePoints, string seriesProgress)
    {
      const string query = @"UPDATE [NRAG]
SET [NRAG].[Tier] = @tier, [NRAG].[Rank] = @rank, [NRAG].[LeaguePoints] = @leaguePoints, [NRAG].[SeriesProgress] = @seriesProgress
FROM [NewRiotAccountGames] [NRAG]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NRAG].[Id]
WHERE [NCRAG].[Key] = @accountKey;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountKey, tier, rank, leaguePoints, seriesProgress });

      return rows == 1;
    }

    public async Task<bool> NewIsAccountGameAssignedToChannel(long channelId, Game game, Server server, string summonerName)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT TOP 1 *
  FROM [NewChannelRiotAccountGames] [NCRAG]
  INNER JOIN [NewRiotAccountGames] [NRAG] ON [NRAG].[Id] = [NCRAG].[RiotAccountGameId]
  INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
  WHERE [NCRAG].[ChannelId] = @channelId AND [NRA].[ServerStr] = @serverStr AND [NRAG].[GameStr] = @gameStr AND [NRA].[NormalizedSummonerName] = @normalizedSummonerName
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleAsync<bool>(query, new {channelId, gameStr, serverStr, normalizedSummonerName});

      return result;
    }

    public async Task<bool> NewExistsAccountGame(Game game, Server server, string summonerName)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT TOP 1 *
  FROM [NewRiotAccountGames] [NRAG]
  INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
  WHERE [NRA].[ServerStr] = @serverStr AND [NRAG].[GameStr] = @gameStr AND [NRA].[NormalizedSummonerName] = @normalizedSummonerName
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleAsync<bool>(query, new { gameStr, serverStr, normalizedSummonerName });

      return result;
    }

    public async Task<bool> NewAssignAccountGameToChannel(long channelId, Game game, Server server, string summonerName, string accountKey)
    {
      var gameStr = game.ToString();
      var serverStr = server.ToString();
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"DECLARE @RiotAccountGameId BIGINT = NULL;

SELECT TOP 1 @RiotAccountGameId = [NRAG].[Id]
FROM [NewRiotAccountGames] [NRAG]
INNER JOIN [NewRiotAccounts] [NRA] ON [NRA].[Id] = [NRAG].[RiotAccountId]
WHERE [NRAG].[GameStr] = @gameStr AND [NRA].[ServerStr] = @serverStr AND [NRA].[NormalizedSummonerName] = @normalizedSummonerName;

INSERT INTO [NewChannelRiotAccountGames] ([ChannelId], [RiotAccountGameId], [Key], [DisplayName], [Active])
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

    public async Task<string> NewGetAccountSummonerName(Server server, string normalizedSummonerName)
    {
      var serverStr = server.ToString();

      const string query = @"SELECT TOP 1 [SummonerName]
FROM [NewRiotAccounts]
WHERE [ServerStr] = @serverStr AND [NormalizedSummonerName] = @normalizedSummonerName;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleAsync<string>(query, new {serverStr, normalizedSummonerName});

      return result;
    }
  }
}
