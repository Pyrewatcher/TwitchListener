using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.DataAccess
{
  public class TftMatchRepository : Repository<TftMatch>
  {
    public override string TableName
    {
      get => "TftMatches";
    }

    public TftMatchRepository(IConfiguration config, ILogger<Repository<TftMatch>> logger) : base(config, logger) { }
    

    public async Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches, long accountId)
    {
      const string query = @"SELECT [MatchId]
FROM [TftMatches]
WHERE [AccountId] = @accountId AND [MatchId] IN @matches;";

      using var connection = CreateConnection();

      var result = (await connection.QueryAsync<string>(query, new {accountId, matches})).ToList();

      var notInDatabase = matches.Where(x => !result.Contains(x));

      return notInDatabase;
    }

    public async Task<bool> InsertFromDto(long accountId, string matchId, TftMatchV1Dto match, TftMatchParticipantV1Dto participant)
    {
      const string query = @"INSERT INTO [TftMatches] ([MatchId], [AccountId], [Timestamp], [Place])
VALUES (@matchId, @accountId, @timestamp, @place);";

      using var connection = CreateConnection();

      var rows = await connection.ExecuteAsync(query, new
      {
        matchId,
        accountId,
        timestamp = match.Info.Timestamp,
        place = participant.Place
      });

      return rows == 1;
    }
  }
}
