using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class SubgiftAction : IAction
  {
    private readonly TwitchClient _client;
    private readonly ILogger<SubgiftAction> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly IUsersRepository _usersRepository;

    public SubgiftAction(TwitchClient client, ILogger<SubgiftAction> logger, IBroadcastersRepository broadcastersRepository,
                         ISubscriptionsRepository subscriptionsRepository, IUsersRepository usersRepository)
    {
      _client = client;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _subscriptionsRepository = subscriptionsRepository;
      _usersRepository = usersRepository;
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

      var broadcaster = await _broadcastersRepository.GetByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{gifterName} PogChamp @{recipientName} {broadcaster.SubGreetingEmote}");
      }

      // Add gifter
      var gifter = await _usersRepository.GetUserById(gifterId);

      if (gifter is null)
      {
        gifter = new User(gifterId, gifterName);
        var inserted = await _usersRepository.InsertUser(gifter);

        if (inserted)
        {
          _logger.LogInformation("User {user} inserted to the database", gifterName);
        }
        else
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _usersRepository.UpdateNameById(gifterId, gifterName);

        if (!updated)
        {
          // TODO: Log failure
        }
      }

      // Add recipient
      var recipient = await _usersRepository.GetUserById(recipientId);

      if (recipient is null)
      {
        recipient = new User(recipientId, recipientName);
        var inserted = await _usersRepository.InsertUser(recipient);

        if (inserted)
        {
          _logger.LogInformation("User {user} inserted to the database", recipientName);
        }
        else
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _usersRepository.UpdateNameById(recipientId, recipientName);

        if (!updated)
        {
          // TODO: Log failure
        }
      }

      if (await _subscriptionsRepository.ExistsByUserId(broadcaster.Id, recipientId))
      {
        var updated = await _subscriptionsRepository.UpdateByUserId(broadcaster.Id, recipientId, MsgId, args["msg-param-sub-plan"], DateTime.UtcNow);

        if (!updated)
        {
          // TODO: Log failure
        }
      }
      else
      {
        var inserted = await _subscriptionsRepository.InsertByUserId(broadcaster.Id, recipientId, MsgId, args["msg-param-sub-plan"], DateTime.UtcNow);

        if (!inserted)
        {
          // TODO: Log failure
        }
      }
    }
  }
}
