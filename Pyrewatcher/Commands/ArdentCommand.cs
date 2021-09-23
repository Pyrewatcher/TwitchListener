using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ArdentCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly CommandVariableRepository _commandVariables;
    private readonly ILogger<ArdentCommand> _logger;

    public ArdentCommand(TwitchClient client, ILogger<ArdentCommand> logger, CommandRepository commands, CommandVariableRepository commandVariables)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
      _commandVariables = commandVariables;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      if (!(message.UserId == "103012193" || message.UserId == "215085185"))
      {
        _logger.LogInformation("Sender is not Szimiszom nor Scytlee_ - returning");

        return false;
      }

      var command = await _commands.FindAsync("Name = @Name", new Command {Name = "ardent"});

      var bodyVariable = await _commandVariables.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                           new CommandVariable {CommandId = command.Id, Name = "text"});
      _client.SendMessage(message.Channel, bodyVariable.Value);

      return true;
    }
  }
}
