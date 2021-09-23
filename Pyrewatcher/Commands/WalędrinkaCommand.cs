using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class WalędrinkaCommand : ICommand
  {
    public List<string> CommandArguments { get; } = new();
    private readonly TwitchClient _client;
    private readonly ILogger<WalędrinkaCommand> _logger;
    private readonly TwitchApiHelper _twitchApiHelper;

    public WalędrinkaCommand(TwitchClient client, ILogger<WalędrinkaCommand> logger, TwitchApiHelper twitchApiHelper)
    {
      _client = client;
      _logger = logger;
      _twitchApiHelper = twitchApiHelper;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var random = new Random();

      Globals.BroadcasterViewers.TryGetValue(message.Channel.ToLower(), out var chatters);

      string user1 = message.DisplayName, user2;

      if (chatters is not null && chatters.Count > 0)
      {
        do
        {
          user2 = chatters[random.Next(chatters.Count)];
        } while (user2 == user1.ToLower() && chatters.Count > 1);
      }
      else
      {
        user2 = user1.ToLower();
      }

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["walędrinka_response"], user1, user2));

      return Task.FromResult(true);
    }
  }
}
