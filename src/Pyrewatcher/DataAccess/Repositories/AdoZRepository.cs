using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class AdoZRepository : RepositoryBase, IAdoZRepository
  {
    public AdoZRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IEnumerable<AdoZEntry>> GetAllEntriesAsync()
    {
      const string query = @"SELECT [c].[Name] AS [ChampionName], [az].[GameWon], [az].[Duration], [az].[Kills], [az].[Deaths], [az].[Assists]
FROM [A-Z] [az]
INNER JOIN [LolChampions] [c] ON [c].[Id] = [az].[ChampionId]
ORDER BY [c].[Name];";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryAsync<AdoZEntry>(query);

      return result;
    }
  }
}
