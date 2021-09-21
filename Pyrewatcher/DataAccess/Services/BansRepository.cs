using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;

namespace Pyrewatcher.DataAccess.Services
{
  public class BansRepository : RepositoryBase, IBansRepository
  {
    public BansRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<bool> IsUserBannedByIdAsync(long userId)
    {
      const string query = "SELECT CASE WHEN EXISTS (SELECT * FROM [Bans] WHERE [UserId] = @userId) THEN 1 ELSE 0 END;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { userId });

      return result;
    }

    public async Task<bool> BanUserByIdAsync(long userId)
    {
      const string query = @"INSERT INTO [Bans] ([UserId]) VALUES (@userId)";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { userId });

      return rows == 1;
    }

    public async Task<bool> UnbanUserByIdAsync(long userId)
    {
      const string query = "DELETE FROM [Bans] WHERE [UserId] = @userId";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { userId });

      return rows == 1;
    }
  }
}
