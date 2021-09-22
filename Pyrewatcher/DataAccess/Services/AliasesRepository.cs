using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Services
{
  public class AliasesRepository : RepositoryBase, IAliasesRepository
  {
    public AliasesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<string>> GetAliasesForCommandByBroadcasterIdAsync(string command, long broadcasterId)
    {
      const string query = @"SELECT [Name]
FROM [Aliases]
WHERE [NewName] = @command AND [BroadcasterId] = @broadcasterId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new { command, broadcasterId });

      return result;
    }

    public async Task<IEnumerable<string>> GetGlobalAliasesForCommandAsync(string command)
    {
      const string query = @"SELECT [Name]
FROM [Aliases]
WHERE [NewName] = @command AND [BroadcasterId] = 0;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new { command });

      return result;
    }

    public async Task<bool> ExistsAnyAliasWithNameByBroadcasterIdAsync(string name, long broadcasterId)
    {
      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Aliases]
  WHERE [Name] = @name AND ([BroadcasterId] = 0 OR [BroadcasterId] = @broadcasterId)
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { name, broadcasterId });

      return result;
    }

    public async Task<bool> CreateChannelAliasAsync(long broadcasterId, string name, string command)
    {
      const string query = @"INSERT INTO [Aliases] ([Name], [NewName], [BroadcasterId])
VALUES (@name, @command, @broadcasterId);";

      var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { name, command, broadcasterId });

      return rows == 1;
    }

    public async Task<bool> ExistsAnyAliasWithNameAsync(string name)
    {
      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Aliases]
  WHERE [Name] = @name
) THEN 1 ELSE 0 END;";

      var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { name });

      return result;
    }

    public async Task<bool> CreateGlobalAliasAsync(string name, string command)
    {
      const string query = @"INSERT INTO [Aliases] ([Name], [NewName], [BroadcasterId])
VALUES (@name, @command, 0);";

      var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { name, command });

      return rows == 1;
    }

    public async Task<long?> GetAliasIdWithNameByBroadcasterIdAsync(string name, long broadcasterId)
    {
      const string query = @"SELECT [Id]
FROM [Aliases]
WHERE [Name] = @name AND ([BroadcasterId] = 0 OR [BroadcasterId] = @broadcasterId);";

      var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<long>(query, new { name, broadcasterId });

      return result;
    }

    public async Task<bool> DeleteByIdAsync(long aliasId)
    {
      const string query = @"DELETE FROM [Aliases]
WHERE [Id] = @aliasId;";

      var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { aliasId });

      return rows == 1;
    }

    public async Task<string> GetAliasCommandWithNameByBroadcasterIdAsync(string name, long broadcasterId)
    {
      const string query = @"SELECT [NewName]
FROM [Aliases]
WHERE [Name] = @name AND ([BroadcasterId] = 0 OR [BroadcasterId] = @broadcasterId);";

      var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<string>(query, new { name, broadcasterId });

      return result;
    }
  }
}
