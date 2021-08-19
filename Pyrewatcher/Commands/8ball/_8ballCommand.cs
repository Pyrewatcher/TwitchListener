using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class _8ballCommand : CommandBase<_8ballCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly CommandVariableRepository _commandVariables;
    private readonly ILogger<_8ballCommand> _logger;

    public _8ballCommand(TwitchClient client, ILogger<_8ballCommand> logger, CommandRepository commands, CommandVariableRepository commandVariables)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
      _commandVariables = commandVariables;
    }

    public override _8ballCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["8ball_usage"], message.DisplayName));
        _logger.LogInformation("Question not provided - returning");

        return null;
      }

      var args = new _8ballCommandArguments {Question = string.Join(' ', argsList)};

      return args;
    }

    public override async Task<bool> ExecuteAsync(_8ballCommandArguments args, ChatMessage message)
    {
      var command = await _commands.FindAsync("Name = @Name", new Command {Name = "8ball"});

      var numberOfResponsesVariable = await _commandVariables.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                                        new CommandVariable {CommandId = command.Id, Name = "numberOfResponses"});
      var responseNumber = Math.Abs(args.Question.GetHashCode()) % int.Parse(numberOfResponsesVariable.Value) + 1;

      _client.SendMessage(message.Channel,
                          string.Format(Globals.Locale["8ball_response"], message.DisplayName, Globals.Locale[$"8ball_{responseNumber}"]));

      return true;
    }
  }
}
