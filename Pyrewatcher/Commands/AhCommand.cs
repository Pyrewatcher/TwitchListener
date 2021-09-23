using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class AhCommandArguments
  {
    public int Value { get; set; }
  }

  public class AhCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<AhCommand> _logger;

    public AhCommand(TwitchClient client, ILogger<AhCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    private AhCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ah_valueTip"], message.DisplayName));
        _logger.LogInformation("Value not provided - returning");

        return null;
      }

      if (!int.TryParse(argsList[0], out var value))
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ah_valueTip"], message.DisplayName));
        _logger.LogInformation("Provided value is invalid: {value} - returning", argsList[0]);

        return null;
      }

      if (value is < 0 or > 500)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ah_valueTip"], message.DisplayName));
        _logger.LogInformation("Value has to be between 0 and 500 - returning");

        return null;
      }

      var args = new AhCommandArguments {Value = value};

      return args;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return Task.FromResult(false);
      }

      var cdrValue = ConvertAhToCdr(args.Value);
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["ah_tocdr_response"], message.DisplayName, args.Value, cdrValue));

      return Task.FromResult(true);
    }

    private double ConvertAhToCdr(int ah)
    {
      return ah == 0 ? 0.0 : Math.Round((1 - 1 / (1 + ah / 100.0)) * 100, 0);
    }
  }
}
