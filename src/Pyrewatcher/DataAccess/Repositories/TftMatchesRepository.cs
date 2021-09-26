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

    public async Task<IEnumerable<string>> NewGetMatchesNotInDatabase(List<string> matches)
    {
      const string query = @"SELECT [StringId]
FROM [NewTftMatches]
WHERE [StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new { matches })).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<IEnumerable<string>> NewGetMatchesToUpdateByKey(string accountKey, List<string> matches)
    {
      const string query = @"SELECT [NTM].[StringId]
FROM [NewTftMatches] [NTM]
INNER JOIN [NewTftMatchPlayers] [NTMP] ON [NTMP].[TftMatchId] = [NTM].[Id]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NTMP].[RiotAccountGameId]
WHERE [NCRAG].[Key] = @accountKey AND [NTM].[StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new { accountKey, matches });

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> NewInsertMatchFromDto(string matchId, TftMatchV1Dto match)
    {
      const string query = @"INSERT INTO [NewTftMatches] ([StringId], [GameStartTimestamp])
VALUES (@matchId, @timestamp);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(match.Info.Timestamp)
      });

      return rows == 1;
    }

    public async Task<bool> NewInsertMatchPlayerFromDto(string accountKey, string matchId, TftMatchParticipantV1Dto player)
    {
      const string query = @"DECLARE @TftMatchId BIGINT;
DECLARE @RiotAccountGameId BIGINT;

SELECT @TftMatchId = [Id]
FROM [NewTftMatches]
WHERE [StringId] = @matchId;

SELECT @RiotAccountGameId = [RiotAccountGameId]
FROM [NewChannelRiotAccountGames]
WHERE [Key] = @accountKey;

INSERT INTO [NewTftMatchPlayers] ([TftMatchId], [RiotAccountGameId], [Place])
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

    public async Task<IEnumerable<NewTftMatch>> NewGetTodaysMatchesByChannelId(long channelId)
    {
      var timestamp = RiotUtilities.GetStartTime();

      const string query = @"SELECT [NTMP].[Place]
FROM [NewTftMatches] [NTM]
INNER JOIN [NewTftMatchPlayers] [NTMP] ON [NTMP].[TftMatchId] = [NTM].[Id]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NTMP].[RiotAccountGameId]
WHERE [NCRAG].[ChannelId] = @channelId AND [NTM].[GameStartTimestamp] >= @timestamp
ORDER BY [NTM].[GameStartTimestamp];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewTftMatch>(query, new { channelId, timestamp });

      return result;
    }
  }
}
