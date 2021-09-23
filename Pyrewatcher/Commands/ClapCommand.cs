using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class ClapCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<ClapCommand> _logger;

    private readonly CommandRepository _commandsRepository;
    private readonly CommandVariableRepository _commandVariablesRepository;

    public ClapCommand(TwitchClient client, ILogger<ClapCommand> logger, CommandRepository commandsRepository,
                       CommandVariableRepository commandVariablesRepository)
    {
      _client = client;
      _logger = logger;
      _commandsRepository = commandsRepository;
      _commandVariablesRepository = commandVariablesRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      if (message.UserId is not ("105167614" or "215085185"))
      {
        _logger.LogInformation("Sender is not dariazpiwnicy nor Scytlee_ - returning");

        return false;
      }

      var command = await _commandsRepository.FindAsync("Name = @Name", new Command {Name = "clap"});

      var bodyVariable = await _commandVariablesRepository.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                           new CommandVariable {CommandId = command.Id, Name = "text"});
      _client.SendMessage(message.Channel, bodyVariable.Value);

      return true;
    }
  }
}
