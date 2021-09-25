using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class SubgiftAction : IAction
  {
    private readonly TwitchClient _client;
    private readonly ILogger<SubgiftAction> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly UserRepository _usersRepository;

    public SubgiftAction(TwitchClient client, ILogger<SubgiftAction> logger, IBroadcastersRepository broadcastersRepository,
                         ISubscriptionsRepository subscriptionsRepository, UserRepository usersRepository)
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
      var gifter = await _usersRepository.FindAsync("Id = @Id", new User {Id = gifterId});

      if (gifter == null)
      {
        gifter = new User {Name = gifterName.ToLower(), DisplayName = gifterName, Id = gifterId};
        await _usersRepository.InsertAsync(gifter);
        _logger.LogInformation("User {user} inserted to the database", gifterName);
      }
      else
      {
        gifter.DisplayName = gifterName;
        gifter.Name = gifterName.ToLower();
        await _usersRepository.UpdateAsync(gifter);
      }

      // Add recipient
      var recipient = await _usersRepository.FindAsync("Id = @Id", new User {Id = recipientId});

      if (recipient == null)
      {
        recipient = new User {Name = recipientName.ToLower(), DisplayName = recipientName, Id = recipientId};
        await _usersRepository.InsertAsync(recipient);
        _logger.LogInformation("User {user} inserted to the database", recipientName);
      }
      else
      {
        recipient.DisplayName = recipientName;
        recipient.Name = recipientName.ToLower();
        await _usersRepository.UpdateAsync(recipient);
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
