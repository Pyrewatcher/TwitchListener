using System.Threading.Tasks;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IUsersRepository
  {
    Task<bool> ExistsById(long userId);
    Task<User> GetUserById(long userId);
    Task<User> GetUserByName(string userName);
    Task<bool> InsertUser(User user);
    Task<bool> UpdateNameById(long userId, string displayName);
  }
}