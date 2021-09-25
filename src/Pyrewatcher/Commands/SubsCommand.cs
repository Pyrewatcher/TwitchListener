using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
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

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ISubscriptionsRepository _subscriptionsRepository;

    public SubsCommand(TwitchClient client, ILogger<SubsCommand> logger, IBroadcastersRepository broadcastersRepository,
                       ISubscriptionsRepository subscriptionsRepository)
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
        broadcaster = await _broadcastersRepository.GetByNameAsync(args.Broadcaster);
      }
      else
      {
        broadcaster = await _broadcastersRepository.GetByNameAsync(message.Channel);
      }

      if (broadcaster == null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["subs_broadcasterDoesNotExist"], message.DisplayName, args.Broadcaster));
        _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

        return false;
      }

      var subscriptions = await _subscriptionsRepository.GetSubscribersCountByBroadcasterId(broadcaster.Id);

      _client.SendMessage(message.Channel,
                          string.Format(Globals.Locale["subs_response"], message.DisplayName, broadcaster.DisplayName, subscriptions));

      return true;
    }
  }
}
