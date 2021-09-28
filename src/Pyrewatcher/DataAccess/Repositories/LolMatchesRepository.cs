using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.LeagueOfLegends.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class LolMatchesRepository : RepositoryBase, ILolMatchesRepository
  {
    public LolMatchesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<string>> GetMatchesNotInDatabaseAsync(List<string> matches)
    {
      const string query = @"SELECT [StringId]
FROM [LolMatches]
WHERE [StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new {matches})).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<IEnumerable<string>> GetMatchesToUpdateByKeyAsync(string accountKey, List<string> matches)
    {
      const string query = @"SELECT [LM].[StringId]
FROM [LolMatches] [LM]
INNER JOIN [LolMatchPlayers] [LMP] ON [LMP].[LolMatchId] = [LM].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [LMP].[RiotAccountGameId]
WHERE [CRAG].[Key] = @accountKey AND [LM].[StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new {accountKey, matches});

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> InsertMatchFromDtoAsync(string matchId, MatchV5Dto match)
    {
      const string query = @"INSERT INTO [LolMatches] ([StringId], [GameStartTimestamp], [WinningTeam], [Duration])
VALUES (@matchId, @timestamp, @winningTeam, @duration);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(match.Info.Timestamp),
        winningTeam = match.Info.Teams.First(x => x.IsWinningTeam).TeamId,
        duration = match.Info.Duration
      });

      return rows == 1;
    }

    public async Task<bool> InsertMatchPlayerFromDtoAsync(string accountKey, string matchId, MatchParticipantV5Dto player)
    {
      const string query = @"DECLARE @LolMatchId BIGINT;
DECLARE @RiotAccountGameId BIGINT;

SELECT TOP 1 @LolMatchId = [Id]
FROM [LolMatches]
WHERE [StringId] = @matchId;

SELECT TOP 1 @RiotAccountGameId = [RiotAccountGameId]
FROM [ChannelRiotAccountGames]
WHERE [Key] = @accountKey;

INSERT INTO [LolMatchPlayers] ([LolMatchId], [RiotAccountGameId], [Team], [ChampionId], [Kills],
  [Deaths], [Assists], [ControlWardsBought])
VALUES (@LolMatchId, @RiotAccountGameId, @team, @championId, @kills, @deaths, @assists, @controlWardsBought);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        accountKey,
        team = player.Team,
        championId = player.ChampionId,
        kills = player.Kills,
        deaths = player.Deaths,
        assists = player.Assists,
        controlWardsBought = player.VisionWardsBought
      });

      return rows == 1;
    }

    public async Task<IEnumerable<LolMatch>> GetTodaysMatchesByChannelIdAsync(long channelId)
    {
      var timestamp = RiotUtilities.GetStartTime();

      const string query = @"SELECT [LMP].[ChampionId], CASE WHEN [LM].[WinningTeam] = [LMP].[Team] THEN 1 ELSE 0 END AS [WonMatch],
  [LMP].[Kills], [LMP].[Deaths], [LMP].[Assists], [LMP].[ControlWardsBought]
FROM [LolMatches] [LM]
INNER JOIN [LolMatchPlayers] [LMP] ON [LMP].[LolMatchId] = [LM].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [LMP].[RiotAccountGameId]
WHERE [CRAG].[ChannelId] = @channelId AND [LM].[GameStartTimestamp] >= @timestamp AND [LM].[Duration] >= 330000
ORDER BY [LM].[GameStartTimestamp];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<LolMatch>(query, new { channelId, timestamp });

      return result;
    }
  }
}
