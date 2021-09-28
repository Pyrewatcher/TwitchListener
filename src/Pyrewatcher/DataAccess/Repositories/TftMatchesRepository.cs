using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.TeamfightTactics.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class TftMatchesRepository : RepositoryBase, ITftMatchesRepository
  {
    public TftMatchesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<string>> GetMatchesNotInDatabaseAsync(List<string> matches)
    {
      const string query = @"SELECT [StringId]
FROM [TftMatches]
WHERE [StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new { matches })).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<IEnumerable<string>> GetMatchesToUpdateByKeyAsync(string accountKey, List<string> matches)
    {
      const string query = @"SELECT [TM].[StringId]
FROM [TftMatches] [TM]
INNER JOIN [TftMatchPlayers] [TMP] ON [TMP].[TftMatchId] = [TM].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [TMP].[RiotAccountGameId]
WHERE [CRAG].[Key] = @accountKey AND [TM].[StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new { accountKey, matches });

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> InsertMatchFromDtoAsync(string matchId, TftMatchV1Dto match)
    {
      const string query = @"INSERT INTO [TftMatches] ([StringId], [GameStartTimestamp])
VALUES (@matchId, @timestamp);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(match.Info.Timestamp)
      });

      return rows == 1;
    }

    public async Task<bool> InsertMatchPlayerFromDtoAsync(string accountKey, string matchId, TftMatchParticipantV1Dto player)
    {
      const string query = @"DECLARE @TftMatchId BIGINT;
DECLARE @RiotAccountGameId BIGINT;

SELECT TOP 1 @TftMatchId = [Id]
FROM [TftMatches]
WHERE [StringId] = @matchId;

SELECT TOP 1 @RiotAccountGameId = [RiotAccountGameId]
FROM [ChannelRiotAccountGames]
WHERE [Key] = @accountKey;

INSERT INTO [TftMatchPlayers] ([TftMatchId], [RiotAccountGameId], [Place])
VALUES (@TftMatchId, @RiotAccountGameId, @place);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        accountKey,
        place = player.Place
      });

      return rows == 1;
    }

    public async Task<IEnumerable<TftMatch>> GetTodaysMatchesByChannelIdAsync(long channelId)
    {
      var timestamp = RiotUtilities.GetStartTime();

      const string query = @"SELECT [TMP].[Place]
FROM [TftMatches] [TM]
INNER JOIN [TftMatchPlayers] [TMP] ON [TMP].[TftMatchId] = [TM].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [TMP].[RiotAccountGameId]
WHERE [CRAG].[ChannelId] = @channelId AND [TM].[GameStartTimestamp] >= @timestamp
ORDER BY [TM].[GameStartTimestamp];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<TftMatch>(query, new { channelId, timestamp });

      return result;
    }
  }
}
