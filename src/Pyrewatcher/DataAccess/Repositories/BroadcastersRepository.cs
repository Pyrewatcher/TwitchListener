using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class BroadcastersRepository : RepositoryBase, IBroadcastersRepository
  {
    public BroadcastersRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<Broadcaster> GetByNameAsync(string broadcasterName)
    {
      var normalizedBroadcasterName = broadcasterName.ToLower();

      const string query = @"SELECT [b].*, [u].[DisplayName]
FROM [Broadcasters] AS [b]
INNER JOIN [Users] AS [u] ON [u].[Id] = [b].[Id]
WHERE [u].[Name] = @normalizedBroadcasterName;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<Broadcaster>(query, new { normalizedBroadcasterName });

      return result;
    }

    public async Task<IEnumerable<Broadcaster>> GetConnectedAsync()
    {
      const string query = @"SELECT [b].*, [u].[DisplayName]
FROM [Broadcasters] AS [b]
INNER JOIN [Users] AS [u] ON [u].[Id] = [b].[Id]
WHERE [b].[Connected] = 1;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<Broadcaster>(query);

      return result;
    }

    public async Task<bool> ToggleConnectedByIdAsync(long broadcasterId)
    {
      const string query = @"UPDATE [Broadcasters]
SET [Connected] = ~[Connected]
WHERE [Id] = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { broadcasterId });

      return rows == 1;
    }

    public async Task<bool> InsertAsync(long userId)
    {
      const string query = @"INSERT INTO [Broadcasters] ([Id])
VALUES (@userId);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { userId });

      return rows == 1;
    }
  }
}
