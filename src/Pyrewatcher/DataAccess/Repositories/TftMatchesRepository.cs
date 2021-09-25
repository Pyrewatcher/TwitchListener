using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class TftMatchesRepository : RepositoryBase, ITftMatchesRepository
  {
    public TftMatchesRepository(IConfiguration config) : base(config)
    {

    }


    public async Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches, long accountId)
    {
      const string query = @"SELECT [MatchId]
FROM [TftMatches]
WHERE [AccountId] = @accountId AND [MatchId] IN @matches;";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<string>(query, new { accountId, matches })).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> InsertFromDto(long accountId, string matchId, TftMatchV1Dto match, TftMatchParticipantV1Dto participant)
    {
      const string query = @"INSERT INTO [TftMatches] ([MatchId], [AccountId], [Timestamp], [Place])
VALUES (@matchId, @accountId, @timestamp, @place);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        accountId,
        timestamp = match.Info.Timestamp,
        place = participant.Place
      });

      return rows == 1;
    }

    public async Task<IEnumerable<TftMatch>> GetTodaysMatchesByAccountId(long accountId)
    {
      var timestamp = RiotUtilities.GetStartTimeInMilliseconds();

      const string query = @"SELECT [Timestamp], [Place]
FROM [TftMatches]
WHERE [AccountId] = @accountId AND [Timestamp] > @timestamp;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<TftMatch>(query, new {accountId, timestamp});

      return result;
    }
  }
}
