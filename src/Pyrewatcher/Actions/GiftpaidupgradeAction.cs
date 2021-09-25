using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;

namespace Pyrewatcher.Actions
{
  public class GiftpaidupgradeAction : IAction
  {
    private readonly TwitchClient _client;

    private readonly IBroadcastersRepository _broadcastersRepository;

    public GiftpaidupgradeAction(TwitchClient client, IBroadcastersRepository broadcastersRepository)
    {
      _client = client;
      _broadcastersRepository = broadcastersRepository;
    }

    public string MsgId
    {
      get => "giftpaidupgrade";
    }

    public async Task PerformAsync(Dictionary<string, string> args)
    {
      var broadcaster = await _broadcastersRepository.GetByNameAsync(args["broadcaster"]);

      if (broadcaster.SubGreetingsEnabled)
      {
        _client.SendMessage(broadcaster.Name, $"@{args["display-name"]} {broadcaster.SubGreetingEmote}");
      }
    }
  }
}
