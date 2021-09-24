using System;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ISubscriptionsRepository
  {
    Task<bool> ExistsByUserId(long broadcasterId, long userId);
    Task<int> GetSubscribersCountByBroadcasterId(long broadcasterId);
    Task<bool> InsertByUserId(long broadcasterId, long userId, string type, string plan, DateTime endsOn);
    Task<bool> UpdateByUserId(long broadcasterId, long userId, string type, string plan, DateTime endsOn);
  }
}