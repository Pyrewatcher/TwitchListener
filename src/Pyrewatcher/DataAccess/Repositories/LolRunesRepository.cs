using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class LolRunesRepository : RepositoryBase, ILolRunesRepository
  {
    public LolRunesRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IDictionary<long, string>> GetAllAsync()
    {
      const string query = @"SELECT [Id] AS [Key], [Name] AS [Value]
FROM [LolRunes];";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<KeyValuePair<long, string>>(query))
       .ToDictionary(x => x.Key, x => x.Value);

      return result;
    }
  }
}
