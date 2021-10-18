using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LoveCommandArguments
  {
    public string LoveObject { get; set; }
  }

  [UsedImplicitly]
  public class LoveCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<LoveCommand> _logger;

    public LoveCommand(TwitchClient client, ILogger<LoveCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    private LoveCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["love_usage"], message.DisplayName));
        _logger.LogInformation("Love object not provided - returning");

        return null;
      }

      var args = new LoveCommandArguments {LoveObject = string.Join(' ', argsList).TrimStart('@')};

      return args;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return Task.FromResult(false);
      }

      if (args.LoveObject.ToLower().TrimEnd('_') == "pyrewatcher")
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["love_response_pyrewatcher"], message.DisplayName));
      }
      else if (args.LoveObject.ToLower() == "riihne" && message.Username == "scytlee_" ||
               args.LoveObject.ToLower() == "scytlee_" && message.Username == "riihne")
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["love_response"], message.DisplayName, args.LoveObject, 111));
      }
      else
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["love_response"], message.DisplayName, args.LoveObject, RandomizeLove()));
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
