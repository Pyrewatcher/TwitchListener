using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IAliasesRepository
  {
    Task<bool> CreateChannelAliasAsync(long broadcasterId, string name, string command);
    Task<bool> CreateGlobalAliasAsync(string name, string command);
    Task<bool> DeleteByIdAsync(long aliasId);
    Task<bool> ExistsAnyAliasWithNameAsync(string name);
    Task<bool> ExistsAnyAliasWithNameByBroadcasterIdAsync(string name, long broadcasterId);
    Task<string> GetAliasCommandWithNameByBroadcasterIdAsync(string name, long broadcasterId);
    Task<IEnumerable<string>> GetAliasesForCommandByBroadcasterIdAsync(string command, long broadcasterId);
    Task<long?> GetAliasIdWithNameByBroadcasterIdAsync(string name, long broadcasterId);
    Task<IEnumerable<string>> GetGlobalAliasesForCommandAsync(string command);
  }
}