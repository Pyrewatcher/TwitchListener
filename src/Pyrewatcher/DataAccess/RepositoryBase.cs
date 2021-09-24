using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Pyrewatcher.DataAccess
{
  public abstract class RepositoryBase
  {
    private readonly IConfiguration _config;

    protected RepositoryBase(IConfiguration config)
    {
      _config = config;
    }

    private SqlConnection SqlConnection()
    {
      return new SqlConnection(_config.GetConnectionString("Database"));
    }

    protected async Task<IDbConnection> CreateConnectionAsync()
    {
      var conn = SqlConnection();
      await conn.OpenAsync();

      return conn;
    }
  }
}
