using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IBroadcastersRepository
  {
    Task<Broadcaster> GetByNameAsync(string broadcasterName);
    Task<IEnumerable<Broadcaster>> GetConnectedAsync();
    Task<bool> InsertAsync(long userId);
    Task<bool> ToggleConnectedByIdAsync(long broadcasterId);
  }
}