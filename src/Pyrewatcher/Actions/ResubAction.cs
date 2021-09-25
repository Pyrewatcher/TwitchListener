using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class ResubAction : IAction
  {
    private readonly TwitchClient _client;
    private readonly ILogger<ResubAction> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly IUsersRepository _usersRepository;

    public ResubAction(TwitchClient client, ILogger<ResubAction> logger, IBroadcastersRepository broadcastersRepository,
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
      get => "resub";
    }

    public async Task PerformAsync(Dictionary<string, string> args)
    {
      var userId = long.Parse(args["user-id"]);
      var userName = args["display-name"];

      var broadcaster = await _broadcastersRepository.GetByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{userName} {broadcaster.SubGreetingEmote}");
      }
      
      var user = await _usersRepository.GetUserById(userId);

      if (user is null)
      {
        user = new User(userId, userName);
        var inserted = await _usersRepository.InsertUser(user);

        if (inserted)
        {
          _logger.LogInformation("User {user} inserted to the database", userName);
        }
        else
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _usersRepository.UpdateNameById(userId, userName);

        if (!updated)
        {
          // TODO: Log failure
        }
      }

      if (await _subscriptionsRepository.ExistsByUserId(broadcaster.Id, userId))
      {
        var updated = await _subscriptionsRepository.UpdateByUserId(broadcaster.Id, userId, MsgId, args["msg-param-sub-plan"], DateTime.UtcNow);

        if (!updated)
        {
          // TODO: Log failure
        }
      }
      else
      {
        var inserted = await _subscriptionsRepository.InsertByUserId(broadcaster.Id, userId, MsgId, args["msg-param-sub-plan"], DateTime.UtcNow);

        if (!inserted)
        {
          // TODO: Log failure
        }
      }
    }
  }
}
