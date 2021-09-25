using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Repositories
{
  public class UsersRepository : RepositoryBase, IUsersRepository
  {
    public UsersRepository(IConfiguration config) : base(config)
    {

    }

    public async Task<User> GetUserById(long userId)
    {
      const string query = @"SELECT [Name], [DisplayName], [Role]
FROM [Users]
WHERE [Id] = @userId;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<User>(query, new { userId });

      return result;
    }

    public async Task<bool> InsertUser(User user)
    {
      const string query = @"INSERT INTO [Users] ([Id], [Name], [DisplayName], [Role])
VALUES (@Id, @Name, @DisplayName, @Role);";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, user);

      return rows == 1;
    }

    public async Task<bool> UpdateNameById(long userId, string displayName)
    {
      var name = displayName.ToLower();

      const string query = @"UPDATE [Users]
SET [Name] = @name, [DisplayName] = @displayName
WHERE [Id] = @userId;";

      using var connection = await CreateConnectionAsync();

      var rows = await connection.ExecuteAsync(query, new { name, displayName, userId });

      return rows == 1;
    }

    public async Task<User> GetUserByName(string userName)
    {
      var normalizedUserName = userName.ToLower();

      const string query = @"SELECT [Id], [Name], [DisplayName], [Role]
FROM [Users]
WHERE [Name] = @normalizedUserName;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QuerySingleOrDefaultAsync<User>(query, new { normalizedUserName });

      return result;
    }

    public async Task<bool> ExistsById(long userId)
    {
      const string query = @"SELECT CASE WHEN EXISTS (
  SELECT *
  FROM [Users]
  WHERE [Id] = @userId
) THEN 1 ELSE 0 END;";

      using var connection = await CreateConnectionAsync();

      var result = await connection.QueryFirstAsync<bool>(query, new { userId });

      return result;
    }
  }
}
