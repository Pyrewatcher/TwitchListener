using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public interface ICommand
  {
    Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message);
  }
}
