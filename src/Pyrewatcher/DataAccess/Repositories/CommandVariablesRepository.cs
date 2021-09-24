using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class CommandVariablesRepository : RepositoryBase, ICommandVariablesRepository
  {
    public CommandVariablesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<string> GetCommandTextById(long commandId)
    {
      const string query = @"SELECT [Value]
FROM [CommandVariables]
WHERE [CommandId] = @commandId AND [Name] = 'text';";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstOrDefaultAsync<string>(query, new { commandId });

      return result;
    }
  }
}
