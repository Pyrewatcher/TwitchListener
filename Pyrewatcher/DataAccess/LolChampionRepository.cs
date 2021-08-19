using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class LolChampionRepository : Repository<LolChampion>
  {
    public override string TableName
    {
      get => "LolChampions";
    }

    public LolChampionRepository(IConfiguration config, ILogger<Repository<LolChampion>> logger) : base(config, logger) { }

    public async Task<IEnumerable<LolChampion>> FindRangeByIdAsync(IEnumerable<long> ids)
    {
      var query = @$"SELECT *
FROM {TableName}
WHERE [Id] IN @ids";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      var response = await connection.QueryAsync<LolChampion>(query, new {ids});

      return response;
    }
  }
}
