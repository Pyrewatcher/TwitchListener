using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class HelpCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly CommandRepository _commandsRepository;

    public HelpCommand(TwitchClient client, CommandRepository commandsRepository)
    {
      _client = client;
      _commandsRepository = commandsRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var commands = (await _commandsRepository.FindRangeAsync("(Channel = '' OR Channel = @Channel) AND IsPublic = 1 AND Name != 'help'",
                                                     new Command {Channel = message.Channel})).Select(x => x.Name)
                                                                                              .OrderBy(x => x)
                                                                                              .ToList();

      if (commands.Count == 0)
      {
        _client.SendMessage(message.Channel, Globals.Locale["help_response_empty"]);
      }
      else
      {
        var sb = new StringBuilder();

        foreach (var command in commands)
        {
          sb.Append('\\');
          sb.Append(command);
          sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["help_response"], sb));
      }

      return true;
    }
  }
}
