using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IBansRepository
  {
    Task<bool> BanUserByIdAsync(long userId);
    Task<bool> IsUserBannedByIdAsync(long userId);
    Task<bool> UnbanUserByIdAsync(long userId);
  }
}