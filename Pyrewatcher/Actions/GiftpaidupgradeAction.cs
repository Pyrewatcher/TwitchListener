using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class GiftpaidupgradeAction : IAction
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly ILogger<GiftpaidupgradeAction> _logger;

    public GiftpaidupgradeAction(TwitchClient client, ILogger<GiftpaidupgradeAction> logger, BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _broadcasters = broadcasters;
    }

    public string MsgId
    {
      get => "giftpaidupgrade";
    }

    public async Task PerformAsync(Dictionary<string, string> args)
    {
      var broadcaster = await _broadcasters.FindWithNameByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{args["display-name"]} {broadcaster.SubGreetingEmote}");
      }
    }
  }
}
