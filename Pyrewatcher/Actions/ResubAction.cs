using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class ResubAction : IAction
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly ILogger<ResubAction> _logger;
    private readonly SubscriptionRepository _subscriptions;
    private readonly UserRepository _users;

    public ResubAction(TwitchClient client, ILogger<ResubAction> logger, UserRepository users, SubscriptionRepository subscriptions,
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
      get => "resub";
    }

    public async Task PerformAsync(Dictionary<string, string> args)
    {
      var userId = long.Parse(args["user-id"]);
      var userName = args["display-name"];

      var broadcaster = await _broadcasters.FindWithNameByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{userName} {broadcaster.SubGreetingEmote}");
      }

      var user = await _users.FindAsync("Id = @Id", new User {Id = userId});

      if (user == null)
      {
        user = new User {Name = userName.ToLower(), DisplayName = userName, Id = userId};
        await _users.InsertAsync(user);
        _logger.LogInformation("User {user} inserted to the database", userName);
      }
      else
      {
        user.DisplayName = userName;
        user.Name = userName.ToLower();
        await _users.UpdateAsync(user);
      }

      var subscription = await _subscriptions.FindAsync("BroadcasterId = @BroadcasterId AND UserId = @UserId",
                                                        new Subscription {BroadcasterId = broadcaster.Id, UserId = userId});

      if (subscription == null)
      {
        subscription = new Subscription
        {
          BroadcasterId = broadcaster.Id,
          EndingTimestamp = new DateTimeOffset(DateTime.UtcNow.AddMonths(1)).ToUnixTimeMilliseconds(),
          Type = MsgId,
          Plan = args["msg-param-sub-plan"],
          UserId = userId
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
