using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class LpCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly IRiotAccountsRepository _riotAccountsRepository;

    public LpCommand(TwitchClient client, IRiotAccountsRepository riotAccountsRepository)
    {
      _client = client;
      _riotAccountsRepository = riotAccountsRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var channelId = long.Parse(message.RoomId);
      var ranks = (await _riotAccountsRepository.GetTodaysRankChangesByChannelIdAsync(channelId)).ToList();

      if (ranks.Any())
      {
        var rankGroupings = ranks.GroupBy(x => x.DisplayName);

        var response = rankGroupings.Select(grouping => $"{grouping.Key} ➔ {string.Join(", ", grouping)}").OrderBy(x => x).ToList();

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["lp_show"], string.Join(" | ", response)));
      }
      else
      {
        _client.SendMessage(message.Channel, Globals.Locale["lp_show_empty"]);
      }

      return true;
    }
  }
}
