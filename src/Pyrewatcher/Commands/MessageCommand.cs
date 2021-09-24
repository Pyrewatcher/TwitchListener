using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class MessageCommandArguments
  {
    public string Broadcaster { get; set; }
    public string Message { get; set; }
  }

  [UsedImplicitly]
  public class MessageCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<MessageCommand> _logger;

    public MessageCommand(TwitchClient client, ILogger<MessageCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    private MessageCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count < 2)
      {
        switch (argsList.Count)
        {
          // missing Broadcaster
          case 0:
            _logger.LogInformation("Broadcaster not provided - returning");

            break;
          // missing Message
          case 1:
            _logger.LogInformation("Message not provided - returning");

            break;
        }

        return null;
      }

      var args = new MessageCommandArguments {Broadcaster = argsList[0], Message = string.Join(' ', argsList.Skip(1))};

      return args;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return Task.FromResult(false);
      }

      if (!_client.JoinedChannels.Select(x => x.Channel).Contains(args.Broadcaster.ToLower()))
      {
        _logger.LogInformation("Pyrewatcher is not connected to broadcaster {broadcaster} - returning", args.Broadcaster);

        return Task.FromResult(false);
      }

      _client.SendMessage(args.Broadcaster.ToLower(), $"{(message.UserId == "215085185" ? "" : " ")}{args.Message}");

      return Task.FromResult(true);
    }
  }
}
