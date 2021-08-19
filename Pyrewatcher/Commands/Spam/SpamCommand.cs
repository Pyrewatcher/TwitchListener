using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class SpamCommand : CommandBase<SpamCommandArguments>
  {
    public List<string> CommandArguments { get; } = new() {"repeats"};
    private readonly TwitchClient _client;
    private readonly ILogger<SpamCommand> _logger;

    public SpamCommand(TwitchClient client, ILogger<SpamCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    public override SpamCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Repeats not provided - returning");

        return null;
      }

      if (!int.TryParse(argsList[0], out var repeats))
      {
        _logger.LogInformation("Provided repeats amount is invalid: {value} - returning", argsList[0]);

        return null;
      }

      if (repeats < 1 || repeats > 6)
      {
        _logger.LogInformation("Repeats amount has to be between 1 and 6 - returning");

        return null;
      }

      var args = new SpamCommandArguments {Repeats = repeats};

      return args;
    }

    public override async Task<bool> ExecuteAsync(SpamCommandArguments args, ChatMessage message)
    {
      for (var i = 0; i < args.Repeats; i++)
      {
        _client.SendMessage(message.Channel, "PepoCheer LET'S PepoCheer GO PepoCheer DAMIAN PepoCheer");
        await Task.Delay(TimeSpan.FromSeconds(0.1));
      }

      return true;
    }
  }
}
