using System.Data;
using System.Data.SqlClient;
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

    protected IDbConnection CreateConnection()
    {
      var conn = SqlConnection();
      conn.Open();

      return conn;
    }
  }
}
