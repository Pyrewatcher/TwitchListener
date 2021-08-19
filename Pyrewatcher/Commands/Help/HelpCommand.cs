using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class HelpCommand : CommandBase<HelpCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly ILogger<HelpCommand> _logger;

    public HelpCommand(TwitchClient client, ILogger<HelpCommand> logger, CommandRepository commands)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
    }

    public override HelpCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new HelpCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(HelpCommandArguments args, ChatMessage message)
    {
      var commands = (await _commands.FindRangeAsync("(Channel = '' OR Channel = @Channel) AND IsPublic = 1 AND Name != 'help'",
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
