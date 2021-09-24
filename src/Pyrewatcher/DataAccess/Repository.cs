using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Pyrewatcher.DataAccess
{
  public abstract class Repository<T> : IRepository<T> where T : class
  {
    public abstract string TableName { get; }

    private static IEnumerable<PropertyInfo> GetProperties
    {
      get => typeof(T).GetProperties();
    }

    private readonly IConfiguration _config;
    private readonly ILogger<Repository<T>> _logger;

    protected Repository(IConfiguration config, ILogger<Repository<T>> logger)
    {
      _config = config;
      _logger = logger;
    }

    public async Task DeleteAsync(string parameterizedWhereClause, T parameters)
    {
      var query = $"DELETE FROM [{TableName}] WHERE {parameterizedWhereClause}";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      await connection.ExecuteAsync(query, parameters);
    }

    public async Task<T> FindAsync(string parameterizedWhereClause, T parameters)
    {
      var query = $"SELECT * FROM [{TableName}] WHERE {parameterizedWhereClause}";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      return await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
    }

    public async Task<IEnumerable<T>> FindAllAsync()
    {
      var query = $"SELECT * FROM [{TableName}]";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      return await connection.QueryAsync<T>(query);
    }

    public async Task<IEnumerable<T>> FindRangeAsync(string parameterizedWhereClause, T parameters)
    {
      var query = $"SELECT * FROM [{TableName}] WHERE {parameterizedWhereClause}";
      //_logger.LogInformation("Executing query: {query}", query);

      using var connection = CreateConnection();

      return await connection.QueryAsync<T>(query, parameters);
    }

    public async Task InsertAsync(T t)
    {
      var insertQuery = GenerateInsertQuery();
      //_logger.LogInformation("Executing query: {query}", insertQuery);

      using var connection = CreateConnection();

      await connection.ExecuteAsync(insertQuery, t);
    }

    public async Task<int> InsertRangeAsync(IEnumerable<T> list)
    {
      var insertQuery = GenerateInsertQuery();

      //_logger.LogInformation("Executing query: {query}", insertQuery);
      using var connection = CreateConnection();

      var inserted = await connection.ExecuteAsync(insertQuery, list);

      return inserted;
    }

    public async Task UpdateAsync(T t)
    {
      var updateQuery = GenerateUpdateQuery();
      //_logger.LogInformation("Executing query: {query}", updateQuery);

      using var connection = CreateConnection();

      await connection.ExecuteAsync(updateQuery, t);
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

    private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
    {
      return (from prop in listOfProperties
              let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
              where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore" &&
                (attributes[0] as DescriptionAttribute)?.Description != "autoincrement"
              select prop.Name).ToList();
    }

    private string GenerateInsertQuery()
    {
      var insertQuery = new StringBuilder($"INSERT INTO [{TableName}] ");

      insertQuery.Append("(");

      var properties = GenerateListOfProperties(GetProperties);
      properties.ForEach(prop =>
      {
        insertQuery.Append($"[{prop}],");
      });

      insertQuery.Remove(insertQuery.Length - 1, 1).Append(") VALUES (");

      properties.ForEach(prop =>
      {
        insertQuery.Append($"@{prop},");
      });

      insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

      return insertQuery.ToString();
    }

    private string GenerateUpdateQuery()
    {
      var updateQuery = new StringBuilder($"UPDATE [{TableName}] SET ");

      var properties = GenerateListOfProperties(GetProperties);

      properties.ForEach(prop =>
      {
        if (!prop.Equals("Id"))
        {
          updateQuery.Append($"[{prop}]=@{prop},");
        }
      });

      updateQuery.Remove(updateQuery.Length - 1, 1).Append(" WHERE [Id]=@Id");

      return updateQuery.ToString();
    }
  }
}
