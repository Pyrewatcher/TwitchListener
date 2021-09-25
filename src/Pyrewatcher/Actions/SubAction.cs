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
  public class SubAction : IAction
  {
    private readonly TwitchClient _client;
    private readonly ILogger<SubAction> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly UserRepository _usersRepository;

    public SubAction(TwitchClient client, ILogger<SubAction> logger, IBroadcastersRepository broadcastersRepository,
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
      get => "sub";
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

      var user = await _usersRepository.FindAsync("Id = @Id", new User {Id = userId});

      if (user == null)
      {
        user = new User {Name = userName.ToLower(), DisplayName = userName, Id = userId};
        await _usersRepository.InsertAsync(user);
        _logger.LogInformation("User {user} inserted to the database", userName);
      }
      else
      {
        user.DisplayName = userName;
        user.Name = userName.ToLower();
        await _usersRepository.UpdateAsync(user);
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
