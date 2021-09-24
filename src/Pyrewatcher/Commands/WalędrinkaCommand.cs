using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class WalędrinkaCommand : ICommand
  {
    private readonly TwitchClient _client;

    public WalędrinkaCommand(TwitchClient client)
    {
      _client = client;
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
