using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LovemeCommandArguments
  {
    public string LoveSender { get; set; }
  }

  [UsedImplicitly]
  public class LovemeCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<LovemeCommand> _logger;

    public LovemeCommand(TwitchClient client, ILogger<LovemeCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    private LovemeCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["loveme_usage"], message.DisplayName));
        _logger.LogInformation("Love sender not provided - returning");

        return null;
      }

      var args = new LovemeCommandArguments {LoveSender = string.Join(' ', argsList).TrimStart('@')};

      return args;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return Task.FromResult(false);
      }

      if (args.LoveSender.ToLower().TrimEnd('_') == "pyrewatcher")
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["loveme_response_pyrewatcher"], message.DisplayName));
      }
      else if (args.LoveSender.ToLower() == "riihne" && message.Username == "scytlee_" ||
               args.LoveSender.ToLower() == "scytlee_" && message.Username == "riihne")
      {
        _client.SendMessage(message.Channel, $" {string.Format(Globals.Locale["loveme_response"], args.LoveSender, message.DisplayName, 111)}");
      }
      else
      {
        _client.SendMessage(message.Channel, $" {string.Format(Globals.Locale["loveme_response"], args.LoveSender, message.DisplayName, RandomizeLove())}");
      }

      return Task.FromResult(true);
    }

    private static int RandomizeLove()
    {
      var random = new Random();
      var output = random.Next(-1, 102);

      return output;
    }
  }
}
