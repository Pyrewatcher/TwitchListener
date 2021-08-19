using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LoveCommand : CommandBase<LoveCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILogger<LoveCommand> _logger;

    public LoveCommand(TwitchClient client, ILogger<LoveCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    public override LoveCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Love object not provided - returning");

        return null;
      }

      var args = new LoveCommandArguments {LoveObject = string.Join(' ', argsList).TrimStart('@')};

      return args;
    }

    public override Task<bool> ExecuteAsync(LoveCommandArguments args, ChatMessage message)
    {
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
