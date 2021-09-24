using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class LatestCommandExecutionsRepository : RepositoryBase, ILatestCommandExecutionsRepository
  {
    public LatestCommandExecutionsRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<DateTime?> GetLatestExecutionAsync(long broadcasterId, long commandId)
    {
      const string query = @"SELECT TOP 1 [TimestampUtc]
FROM [LatestCommandExecutions]
WHERE [BroadcasterId] = @broadcasterId AND [CommandId] = @commandId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<DateTime?>(query, new { broadcasterId, commandId });

      return result;
    }

    public async Task<bool> InsertLatestExecution(long broadcasterId, long commandId, DateTime timestampUtc)
    {
      const string query = @"INSERT INTO [LatestCommandExecutions] ([BroadcasterId], [CommandId], [TimestampUtc])
VALUES (@broadcasterId, @commandId, @timestampUtc);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { broadcasterId, commandId, timestampUtc });

      return rows == 1;
    }

    public async Task<bool> UpdateLatestExecution(long broadcasterId, long commandId, DateTime timestampUtc)
    {
      const string query = @"UPDATE [LatestCommandExecutions]
SET [TimestampUtc] = @timestampUtc
WHERE [BroadcasterId] = @broadcasterId AND [CommandId] = @commandId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { broadcasterId, commandId, timestampUtc });

      return rows == 1;
    }
  }
}
