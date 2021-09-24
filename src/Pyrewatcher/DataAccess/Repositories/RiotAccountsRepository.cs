using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class RiotAccountsRepository : RepositoryBase, IRiotAccountsRepository
  {
    public RiotAccountsRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<RiotAccount>> GetAccountsByBroadcasterIdAsync(long broadcasterId)
    {
      const string query = @"SELECT [Id], [GameAbbreviation], [SummonerName], [ServerCode], [DisplayName], [Active]
FROM [RiotAccounts]
WHERE [BroadcasterId] = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { broadcasterId });

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveAccountsByBroadcasterIdAsync(long broadcasterId)
    {
      const string query = @"SELECT [Id], [GameAbbreviation], [SummonerName], [ServerCode], [DisplayName]
FROM [RiotAccounts]
WHERE [BroadcasterId] = @broadcasterId AND [Active] = 1;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { broadcasterId });

      return result;
    }

    public async Task<RiotAccount> GetAccountForDisplayByDetailsAsync(string gameAbbreviation, string serverCode, string summonerName, long broadcasterId)
    {
      gameAbbreviation = gameAbbreviation.ToLower();
      serverCode = serverCode.ToUpper();
      summonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"SELECT [GameAbbreviation], [SummonerName], [ServerCode]
FROM [RiotAccounts]
WHERE [GameAbbreviation] = @gameAbbreviation AND [ServerCode] = @serverCode
  AND [NormalizedSummonerName] = @summonerName AND [BroadcasterId] = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<RiotAccount>(query, new { gameAbbreviation, serverCode, summonerName, broadcasterId });

      return result;
    }

    public async Task InsertAccount(RiotAccount account)
    {
      const string query = @"INSERT INTO [RiotAccounts] (BroadcasterId, GameAbbreviation, SummonerName, NormalizedSummonerName,
  ServerCode, SummonerId, AccountId, Puuid)
OUTPUT [inserted].[Id]
VALUES (@BroadcasterId, @GameAbbreviation, @SummonerName, @NormalizedSummonerName, @ServerCode, @SummonerId, @AccountId, @Puuid);";

      using var connection = await CreateConnectionAsync();

      account.Id = await connection.ExecuteScalarAsync<long>(query, account);
    }

    public async Task<bool> DeleteByIdAsync(long accountId)
    {
      const string query = "DELETE FROM [RiotAccounts] WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountId });

      return rows == 1;
    }

    public async Task<RiotAccount> GetAccountForDisplayByIdAsync(long accountId)
    {
      const string query = @"SELECT [GameAbbreviation], [SummonerName], [ServerCode]
FROM [RiotAccounts]
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<RiotAccount>(query, new { accountId });

      return result;
    }

    public async Task<bool> ToggleActiveByIdAsync(long accountId)
    {
      const string query = @"UPDATE [RiotAccounts]
SET [Active] = ~[Active]
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountId });

      return rows == 1;
    }

    public async Task<bool> UpdateDisplayNameByIdAsync(long accountId, string displayName)
    {
      const string query = @"UPDATE [RiotAccounts]
SET [DisplayName] = @displayName
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountId, displayName });

      return rows == 1;
    }

    public async Task<bool> UpdateSummonerNameByIdAsync(long accountId, string summonerName)
    {
      var normalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);

      const string query = @"UPDATE [RiotAccounts]
SET [SummonerName] = @summonerName, [NormalizedSummonerName] = @normalizedSummonerName
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountId, summonerName, normalizedSummonerName });

      return rows == 1;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(long broadcasterId)
    {
      const string query = @"SELECT [Id], [SummonerName], [ServerCode], [DisplayName], [SummonerId], [AccountId], [Puuid]
FROM [RiotAccounts]
WHERE [BroadcasterId] = @broadcasterId AND [GameAbbreviation] = 'lol' AND [Active] = 1;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { broadcasterId });

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveAccountsWithRankByBroadcasterIdAsync(long broadcasterId)
    {
      const string query = @"SELECT [GameAbbreviation], [SummonerName], [ServerCode], [DisplayName], [Tier], [Rank], [LeaguePoints], [SeriesProgress]
FROM [RiotAccounts]
WHERE [BroadcasterId] = @broadcasterId AND [GameAbbreviation] IN ('lol', 'tft') AND [Active] = 1;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { broadcasterId });

      return result;
    }

    public async Task<IEnumerable<RiotAccount>> GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(long broadcasterId)
    {
      const string query = @"SELECT [Id], [SummonerName], [ServerCode], [DisplayName], [SummonerId], [AccountId], [Puuid]
FROM [RiotAccounts]
WHERE [BroadcasterId] = @broadcasterId AND [GameAbbreviation] = 'tft' AND [Active] = 1;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<RiotAccount>(query, new { broadcasterId });

      return result;
    }

    public async Task<bool> UpdateRankByIdAsync(long accountId, string tier, string rank, string leaguePoints, string seriesProgress)
    {
      const string query = @"UPDATE [RiotAccounts]
SET [Tier] = @tier, [Rank] = @rank, [LeaguePoints] = @leaguePoints, [SeriesProgress] = @seriesProgress
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { accountId, tier, rank, leaguePoints, seriesProgress });

      return rows == 1;
    }

    public async Task<RiotAccount> GetAccountForApiCallsByIdAsync(long accountId)
    {
      const string query = @"SELECT [Id], [GameAbbreviation], [SummonerName], [ServerCode], [DisplayName], [SummonerId], [AccountId], [Puuid]
FROM [RiotAccounts]
WHERE [Id] = @accountId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<RiotAccount>(query, new { accountId });

      return result;
    }
  }
}
