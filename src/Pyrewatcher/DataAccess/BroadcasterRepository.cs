using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class BroadcasterRepository : Repository<Broadcaster>
  {
    public override string TableName
    {
      get => "Broadcasters";
    }

    public BroadcasterRepository(IConfiguration config, ILogger<Repository<Broadcaster>> logger) : base(config, logger) { }

    public async Task<Broadcaster> FindWithNameByNameAsync(string broadcasterName)
    {
      var query = @$"SELECT b.*, u.DisplayName
FROM {TableName} AS b
INNER JOIN Users AS u ON u.Id = b.Id
WHERE u.Name = @broadcasterName";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      return await connection.QuerySingleOrDefaultAsync<Broadcaster>(query, new {broadcasterName = broadcasterName.ToLower()});
    }

    public async Task<IEnumerable<Broadcaster>> FindWithNameAllConnectedAsync()
    {
      var query = @$"SELECT b.*, u.DisplayName
FROM {TableName} AS b
INNER JOIN Users AS u ON u.Id = b.Id
WHERE b.Connected = 1";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      return await connection.QueryAsync<Broadcaster>(query);
    }
  }
}
