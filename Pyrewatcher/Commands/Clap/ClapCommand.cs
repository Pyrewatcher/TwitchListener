using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ClapCommand : CommandBase<ClapCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly CommandVariableRepository _commandVariables;
    private readonly ILogger<ClapCommand> _logger;

    public ClapCommand(TwitchClient client, ILogger<ClapCommand> logger, CommandRepository commands, CommandVariableRepository commandVariables)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
      _commandVariables = commandVariables;
    }

    public override ClapCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new ClapCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(ClapCommandArguments args, ChatMessage message)
    {
      if (message.UserId is not ("105167614" or "215085185"))
      {
        _logger.LogInformation("Sender is not dariazpiwnicy nor Scytlee_ - returning");

        return false;
      }

      var command = await _commands.FindAsync("Name = @Name", new Command {Name = "clap"});

      var bodyVariable = await _commandVariables.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                           new CommandVariable {CommandId = command.Id, Name = "text"});
      _client.SendMessage(message.Channel, bodyVariable.Value);

      return true;
    }
  }
}
