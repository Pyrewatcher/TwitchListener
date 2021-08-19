using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class PastaCommand : CommandBase<PastaCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly CommandVariableRepository _commandVariables;
    private readonly ILogger<PastaCommand> _logger;

    public PastaCommand(TwitchClient client, ILogger<PastaCommand> logger, CommandRepository commands, CommandVariableRepository commandVariables)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
      _commandVariables = commandVariables;
    }

    public override PastaCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      var args = new PastaCommandArguments();

      if (argsList.Count != 0)
      {
        args.PastaName = argsList[0].ToLower();
      }

      return args;
    }

    public override async Task<bool> ExecuteAsync(PastaCommandArguments args, ChatMessage message)
    {
      var command = await _commands.FindAsync("Name = @Name", new Command {Name = "pasta"});

      if (args.PastaName != null)
      {
        var pastaVariable = await _commandVariables.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                              new CommandVariable {CommandId = command.Id, Name = args.PastaName});

        if (pastaVariable == null)
        {
          _logger.LogInformation("Pasta with name {name} doesn't exist - returning", args.PastaName);

          return false;
        }

        _client.SendMessage(message.Channel, pastaVariable.Value);
      }
      else // lookup list of pastas
      {
        var pastaList = (await _commandVariables.FindRangeAsync("CommandId = @CommandId", new CommandVariable {CommandId = command.Id}))
                       .Select(x => x.Name)
                       .OrderBy(x => x)
                       .ToList();

        _client.SendMessage(message.Channel,
                            pastaList.Count == 0
                              ? Globals.Locale["pasta_response_empty"]
                              : string.Format(Globals.Locale["pasta_response"], string.Join(", ", pastaList)));
      }

      return true;
    }
  }
}
