using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class CommandsRepository : RepositoryBase, ICommandsRepository
  {
    public CommandsRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<bool> ExistsForChannelByName(string command, string broadcasterName)
    {
      var normalizedCommand = command.ToLower();
      var normalizedBroadcasterName = broadcasterName.ToLower();

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Commands]
  WHERE [Name] = @normalizedCommand AND ([Channel] = '' OR [Channel] = @normalizedBroadcasterName)
) THEN 1 ELSE 0 END;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { normalizedCommand, normalizedBroadcasterName });

      return result;
    }

    public async Task<bool> ExistsAnyByName(string command)
    {
      var normalizedCommand = command.ToLower();

      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Commands]
  WHERE [Name] = @normalizedCommand
) THEN 1 ELSE 0 END;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { normalizedCommand });

      return result;
    }

    public async Task<Command> GetCommandByName(string command)
    {
      var normalizedCommand = command.ToLower();

      const string query = @"SELECT [Id], [Name], [Channel], [Type], [IsAdministrative], [IsPublic], [Cooldown], [UsageCount]
FROM [Commands]
WHERE [Name] = @normalizedCommand;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<Command>(query, new { normalizedCommand });

      return result;
    }

    public async Task<bool> UpdateCooldownById(long commandId, int cooldown)
    {
      const string query = @"UPDATE [Commands]
SET [Cooldown] = @cooldown
WHERE [Id] = @commandId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { cooldown, commandId });

      return rows == 1;
    }

    public async Task<IEnumerable<string>> GetCommandNamesForHelp(string broadcasterName)
    {
      var normalizedBroadcasterName = broadcasterName.ToLower();

      const string query = @"SELECT '\' + [Name]
FROM [Commands]
WHERE ([Channel] = '' OR [Channel] = @normalizedBroadcasterName) AND [IsPublic] = 1 AND [Name] != 'help'
ORDER BY [Name];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<string>(query, new { normalizedBroadcasterName });

      return result;
    }

    public async Task<Command> GetCommandForChannelByName(string command, string broadcasterName)
    {
      var normalizedCommand = command.ToLower();
      var normalizedBroadcasterName = broadcasterName.ToLower();

      const string query = @"SELECT [Id], [Name], [Channel], [Type], [IsAdministrative], [IsPublic], [Cooldown], [UsageCount]
FROM [Commands]
WHERE [Name] = @normalizedCommand AND ([Channel] = '' OR [Channel] = @normalizedBroadcasterName);";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<Command>(query, new { normalizedCommand, normalizedBroadcasterName });

      return result;
    }

    public async Task<bool> IncrementUsageCountById(long commandId)
    {
      const string query = @"UPDATE [Commands]
SET [UsageCount] = [UsageCount] + 1
WHERE [Id] = @commandId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { commandId });

      return rows == 1;
    }
  }
}
