using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class HelpCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly ICommandsRepository _commandsRepository;

    public HelpCommand(TwitchClient client, ICommandsRepository commandsRepository)
    {
      _client = client;
      _commandsRepository = commandsRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var commands = (await _commandsRepository.GetCommandNamesForHelp(message.Channel)).ToList();

      _client.SendMessage(message.Channel,
                          commands.Any()
                            ? string.Format(Globals.Locale["help_response"], string.Join(", ", commands))
                            : Globals.Locale["help_response_empty"]);

      return true;
    }
  }
}
