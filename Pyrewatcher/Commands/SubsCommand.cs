using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

  [UsedImplicitly]
  public class SubsCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<SubsCommand> _logger;

    private readonly BroadcasterRepository _broadcastersRepository;
    private readonly SubscriptionRepository _subscriptionsRepository;

    public SubsCommand(TwitchClient client, ILogger<SubsCommand> logger, BroadcasterRepository broadcastersRepository,
                       SubscriptionRepository subscriptionsRepository)
    {
      _client = client;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _subscriptionsRepository = subscriptionsRepository;
    }

    private SubsCommandArguments ParseAndValidateArguments(List<string> argsList)
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
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      Broadcaster broadcaster;

      if (args.Broadcaster != null)
      {
        broadcaster = await _broadcastersRepository.FindWithNameByNameAsync(args.Broadcaster.ToLower());
      }
      else
      {
        broadcaster = await _broadcastersRepository.FindWithNameByNameAsync(message.Channel);
      }

      if (broadcaster == null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["subs_broadcasterDoesNotExist"], message.DisplayName, args.Broadcaster));
        _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

        return false;
      }

      var endingTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

      var subscriptions = (await _subscriptionsRepository.FindRangeAsync("EndingTimestamp >= @EndingTimestamp AND BroadcasterId = @BroadcasterId",
                                                               new Subscription {EndingTimestamp = endingTimestamp, BroadcasterId = broadcaster.Id}))
       .ToList();

      _client.SendMessage(message.Channel,
                          string.Format(Globals.Locale["subs_response"], message.DisplayName, broadcaster.DisplayName, subscriptions.Count));

      return true;
    }
  }
}
