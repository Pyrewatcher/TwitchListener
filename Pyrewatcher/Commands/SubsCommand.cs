using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class SubsCommandArguments
  {
    public string Broadcaster { get; set; }
  }

  public class SubsCommand : ICommand
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly ILogger<SubsCommand> _logger;
    private readonly SubscriptionRepository _subscriptions;

    public SubsCommand(TwitchClient client, ILogger<SubsCommand> logger, SubscriptionRepository subscriptions, BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _subscriptions = subscriptions;
      _broadcasters = broadcasters;
    }

    private SubsCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      var args = new SubsCommandArguments();

      if (argsList.Count != 0)
      {
        args.Broadcaster = argsList[0];
      }

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return false;
      }

      Broadcaster broadcaster;

      if (args.Broadcaster != null)
      {
        broadcaster = await _broadcasters.FindWithNameByNameAsync(args.Broadcaster.ToLower());
      }
      else
      {
        broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);
      }

      if (broadcaster == null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["subs_broadcasterDoesNotExist"], message.DisplayName, args.Broadcaster));
        _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

        return false;
      }

      var endingTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

      var subscriptions = (await _subscriptions.FindRangeAsync("EndingTimestamp >= @EndingTimestamp AND BroadcasterId = @BroadcasterId",
                                                               new Subscription {EndingTimestamp = endingTimestamp, BroadcasterId = broadcaster.Id}))
       .ToList();

      _client.SendMessage(message.Channel,
                          string.Format(Globals.Locale["subs_response"], message.DisplayName, broadcaster.DisplayName, subscriptions.Count));

      return true;
    }
  }
}
