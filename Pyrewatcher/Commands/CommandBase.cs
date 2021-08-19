using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public abstract class CommandBase<TCommandArguments> where TCommandArguments : ICommandArguments
  {
    public abstract TCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message);
    public abstract Task<bool> ExecuteAsync(TCommandArguments args, ChatMessage message);

    public async Task<bool> HandleAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);
      var executionResult = args != null && await ExecuteAsync(args, message);

      return executionResult;
    }
  }
}
