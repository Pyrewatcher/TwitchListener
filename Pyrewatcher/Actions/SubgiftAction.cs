using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class SubgiftAction : IAction
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly ILogger<SubgiftAction> _logger;
    private readonly SubscriptionRepository _subscriptions;
    private readonly UserRepository _users;

    public SubgiftAction(TwitchClient client, ILogger<SubgiftAction> logger, UserRepository users, SubscriptionRepository subscriptions,
                         BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _users = users;
      _subscriptions = subscriptions;
      _broadcasters = broadcasters;
    }

    public string MsgId
    {
      get => "subgift";
    }

    public async Task PerformAsync(Dictionary<string, string> args)
    {
      var gifterId = long.Parse(args["user-id"]);
      var gifterName = args["display-name"];
      var recipientId = long.Parse(args["msg-param-recipient-id"]);
      var recipientName = args["msg-param-recipient-display-name"];

      var broadcaster = await _broadcasters.FindWithNameByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{gifterName} PogChamp @{recipientName} {broadcaster.SubGreetingEmote}");
      }

      // Add gifter
      var gifter = await _users.FindAsync("Id = @Id", new User {Id = gifterId});

      if (gifter == null)
      {
        gifter = new User {Name = gifterName.ToLower(), DisplayName = gifterName, Id = gifterId};
        await _users.InsertAsync(gifter);
        _logger.LogInformation("User {user} inserted to the database", gifterName);
      }
      else
      {
        gifter.DisplayName = gifterName;
        gifter.Name = gifterName.ToLower();
        await _users.UpdateAsync(gifter);
      }

      // Add recipient
      var recipient = await _users.FindAsync("Id = @Id", new User {Id = recipientId});

      if (recipient == null)
      {
        recipient = new User {Name = recipientName.ToLower(), DisplayName = recipientName, Id = recipientId};
        await _users.InsertAsync(recipient);
        _logger.LogInformation("User {user} inserted to the database", recipientName);
      }
      else
      {
        recipient.DisplayName = recipientName;
        recipient.Name = recipientName.ToLower();
        await _users.UpdateAsync(recipient);
      }

      // Add subscription entry
      var subscription = await _subscriptions.FindAsync("BroadcasterId = @BroadcasterId AND UserId = @UserId",
                                                        new Subscription {BroadcasterId = broadcaster.Id, UserId = recipientId});

      if (subscription == null)
      {
        subscription = new Subscription
        {
          BroadcasterId = broadcaster.Id,
          EndingTimestamp = new DateTimeOffset(DateTime.UtcNow.AddMonths(1)).ToUnixTimeMilliseconds(),
          Type = MsgId,
          Plan = args["msg-param-sub-plan"],
          UserId = recipientId
        };
        await _subscriptions.InsertAsync(subscription);
      }
      else
      {
        subscription.EndingTimestamp = new DateTimeOffset(DateTime.UtcNow.AddMonths(1)).ToUnixTimeMilliseconds();
        subscription.Type = MsgId;
        subscription.Plan = args["msg-param-sub-plan"];
        await _subscriptions.UpdateAsync(subscription);
      }
    }
  }
}
