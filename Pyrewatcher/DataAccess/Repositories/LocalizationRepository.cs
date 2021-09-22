using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class LocalizationRepository : RepositoryBase, ILocalizationRepository
  {
    public LocalizationRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<IDictionary<string, string>> GetLocalizationByCode(string localeCode)
    {
      const string query = @"SELECT [Name] AS [Key], [Line] AS [Value]
FROM [LocaleLines]
WHERE [LocaleCode] = @localeCode";

      using var connection = await CreateConnectionAsync();

      var result = (await connection.QueryAsync<KeyValuePair<string, string>>(query, new { localeCode }))
       .ToDictionary(x => x.Key, x => x.Value);

      return result;
    }
  }
}
