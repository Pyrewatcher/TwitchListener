using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class SubscriptionsRepository : RepositoryBase, ISubscriptionsRepository
  {
    public SubscriptionsRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<bool> ExistsByUserId(long broadcasterId, long userId)
    {
      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Subscriptions]
  WHERE [UserId] = @userId AND [BroadcasterId] = @broadcasterId
) THEN 1 ELSE 0 END;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { userId, broadcasterId });

      return result;
    }

    public async Task<bool> UpdateByUserId(long broadcasterId, long userId, string type, string plan, DateTime endsOn)
    {
      var endingTimestamp = new DateTimeOffset(endsOn.AddMonths(1)).ToUnixTimeMilliseconds();

      const string query = @"UPDATE [Subscriptions]
SET [Type] = @type, [Plan] = @plan, [EndingTimestamp] = @endingTimestamp
WHERE [UserId] = @userId AND [BroadcasterId] = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { type, plan, endingTimestamp, userId, broadcasterId });

      return rows == 1;
    }

    public async Task<bool> InsertByUserId(long broadcasterId, long userId, string type, string plan, DateTime endsOn)
    {
      var endingTimestamp = new DateTimeOffset(endsOn.AddMonths(1)).ToUnixTimeMilliseconds();

      const string query = @"INSERT INTO [Subscriptions] ([UserId], [BroadcasterId], [Type], [Plan], [EndingTimestamp])
VALUES (@userId, @broadcasterId, @type, @plan, @endingTimestamp);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { userId, broadcasterId, type, plan, endingTimestamp });

      return rows == 1;
    }

    public async Task<int> GetSubscribersCountByBroadcasterId(long broadcasterId)
    {
      var endingTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

      const string query = @"SELECT COUNT(1)
FROM [Subscriptions]
WHERE EndingTimestamp >= @endingTimestamp AND BroadcasterId = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var count = await connection.QueryFirstAsync<int>(query, new { endingTimestamp, broadcasterId });

      return count;
    }
  }
}
