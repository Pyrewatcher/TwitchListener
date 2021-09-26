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

    public async Task<IEnumerable<string>> NewGetMatchesNotInDatabase(List<string> matches)
    {
      const string query = @"SELECT [StringId]
FROM [NewLolMatches]
WHERE [StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new {matches})).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<IEnumerable<string>> NewGetMatchesToUpdateByKey(string accountKey, List<string> matches)
    {
      const string query = @"SELECT [NLM].[StringId]
FROM [NewLolMatches] [NLM]
INNER JOIN [NewLolMatchPlayers] [NLMP] ON [NLMP].[LolMatchId] = [NLM].[Id]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NLMP].[RiotAccountGameId]
WHERE [NCRAG].[Key] = @accountKey AND [NLM].[StringId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new {accountKey, matches});

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> NewInsertMatchFromDto(string matchId, MatchV5Dto match)
    {
      const string query = @"INSERT INTO [NewLolMatches] ([StringId], [GameStartTimestamp], [WinningTeam], [Duration])
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

    public async Task<bool> NewInsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player)
    {
      const string query = @"DECLARE @LolMatchId BIGINT;
DECLARE @RiotAccountGameId BIGINT;

SELECT @LolMatchId = [Id]
FROM [NewLolMatches]
WHERE [StringId] = @matchId;

SELECT @RiotAccountGameId = [RiotAccountGameId]
FROM [NewChannelRiotAccountGames]
WHERE [Key] = @accountKey;

INSERT INTO [NewLolMatchPlayers] ([LolMatchId], [RiotAccountGameId], [Team], [ChampionId], [Kills],
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

    public async Task<IEnumerable<NewLolMatch>> NewGetTodaysMatchesByChannelId(long channelId)
    {
      var timestamp = RiotUtilities.GetStartTime();

      const string query = @"SELECT [NLMP].[ChampionId], CASE WHEN [NLM].[WinningTeam] = [NLMP].[Team] THEN 1 ELSE 0 END AS [WonMatch],
  [NLMP].[Kills], [NLMP].[Deaths], [NLMP].[Assists], [NLMP].[ControlWardsBought]
FROM [NewLolMatches] [NLM]
INNER JOIN [NewLolMatchPlayers] [NLMP] ON [NLMP].[LolMatchId] = [NLM].[Id]
INNER JOIN [NewChannelRiotAccountGames] [NCRAG] ON [NCRAG].[RiotAccountGameId] = [NLMP].[RiotAccountGameId]
WHERE [NCRAG].[ChannelId] = @channelId AND [NLM].[GameStartTimestamp] >= @timestamp AND [NLM].[Duration] >= 330000
ORDER BY [NLM].[GameStartTimestamp];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<NewLolMatch>(query, new { channelId, timestamp });

      return result;
    }
  }
}
