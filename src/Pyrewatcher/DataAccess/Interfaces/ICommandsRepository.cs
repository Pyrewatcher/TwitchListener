using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ICommandsRepository
  {
    Task<bool> ExistsAnyByName(string command);
    Task<bool> ExistsForChannelByName(string command, string broadcasterName);
    Task<Command> GetCommandByName(string command);
    Task<Command> GetCommandForChannelByName(string command, string broadcasterName);
    Task<IEnumerable<string>> GetCommandNamesForHelp(string broadcasterName);
    Task<bool> IncrementUsageCountById(long commandId);
    Task<bool> UpdateCooldownById(long commandId, int cooldown);
  }
}