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
  public class CooldownCommandArguments
  {
    public string Command { get; set; }
    public int? NewValue { get; set; }
  }

  [UsedImplicitly]
  public class CooldownCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<CooldownCommand> _logger;

    private readonly CommandRepository _commandsRepository;

    public CooldownCommand(TwitchClient client, ILogger<CooldownCommand> logger, CommandRepository commandsRepository)
    {
      _client = client;
      _logger = logger;
      _commandsRepository = commandsRepository;
    }

    private CooldownCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Command not provided - returning");

        return null;
      }

      var args = new CooldownCommandArguments {Command = argsList[0], NewValue = null};

      if (argsList.Count != 1)
      {
        if (!int.TryParse(argsList[1], out var newValue))
        {
          _logger.LogInformation("\"{value}\" is not a valid cooldown value - returning", argsList[1]);

          return null;
        }

        if (newValue is < 0 or > 3600)
        {
          _logger.LogInformation("\"{value}\" is not a value between 0 and 3600 seconds - returning", newValue);

          return null;
        }

        args.NewValue = newValue;
      }

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      var command = await _commandsRepository.FindAsync("Name = @Name", new Command {Name = args.Command.ToLower()});

      if (command == null)
      {
        _logger.LogInformation("There is no command with name {name} - returning", args.Command);

        return false;
      }

      if (command.IsAdministrative)
      {
        _logger.LogInformation("Command {command} is administrative - returning", args.Command);

        return false;
      }

      if (args.NewValue != null) // set command cooldown to the new value
      {
        var oldValue = command.Cooldown;
        command.Cooldown = args.NewValue.Value;
        await _commandsRepository.UpdateAsync(command);
        _client.SendMessage(message.Channel,
                            string.Format(Globals.Locale["cooldown_changed"], message.DisplayName, command.Name, oldValue, command.Cooldown));
      }
      else // lookup command cooldown
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["cooldown_lookup"], message.DisplayName, command.Name, command.Cooldown));
      }

      return true;
    }
  }
}
