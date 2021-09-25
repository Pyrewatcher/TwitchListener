using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class LolMatchesRepository : RepositoryBase, ILolMatchesRepository
  {
    public LolMatchesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches, long accountId)
    {

      const string query = @"SELECT [FullMatchId]
FROM [LolMatches]
WHERE [AccountId] = @accountId AND [FullMatchId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new {accountId, matches})).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> InsertFromDto(long accountId, string fullMatchId, MatchV5Dto match, MatchParticipantV5Dto participant)
    {
      const string query = @"INSERT INTO [LolMatches] ([FullMatchId], [MatchId], [ServerApiCode], [AccountId],
[Timestamp], [Result], [ChampionId], [KDA], [GameDuration], [ControlWardsBought])
VALUES (@fullMatchId, @matchId, @serverApiCode, @accountId, @timestamp,
@result, @championId, @kda, @gameDuration, @controlWardsBought);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        fullMatchId,
        matchId = match.Info.Id,
        serverApiCode = fullMatchId.Split('_')[0],
        accountId,
        timestamp = match.Info.Timestamp,
        result = participant.WonMatch ? "W" : "L",
        championId = participant.ChampionId,
        kda = $"{participant.Kills}/{participant.Deaths}/{participant.Assists}",
        gameDuration = match.Info.Duration / 1000,
        controlWardsBought = participant.VisionWardsBought
      });

      return rows == 1;
    }

    public async Task<IEnumerable<LolMatch>> GetTodaysMatchesByAccountId(long accountId)
    {
      var timestamp = RiotUtilities.GetStartTimeInMilliseconds();

      const string query = @"SELECT [Timestamp], [Result], [ChampionId], [KDA], [ControlWardsBought]
FROM [LolMatches]
WHERE [AccountId] = @accountId AND [Timestamp] > @Timestamp AND [GameDuration] >= 330;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<LolMatch>(query, new { accountId, timestamp });

      return result;
    }
  }
}
