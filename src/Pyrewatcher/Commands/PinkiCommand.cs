using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class PinkiCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly ILolMatchesRepository _lolMatchesRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;

    public PinkiCommand(TwitchClient client, ILolMatchesRepository lolMatchesRepository, IRiotAccountsRepository riotAccountsRepository)
    {
      _client = client;
      _lolMatchesRepository = lolMatchesRepository;
      _riotAccountsRepository = riotAccountsRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<LolMatch>();

      foreach (var account in accounts)
      {
        var accountMatches = await _lolMatchesRepository.GetTodaysMatchesByAccountId(account.Id);
        matches.AddRange(accountMatches);
      }

      if (matches.Any())
      {
        var pinksBought = matches.Sum(x => x.ControlWardsBought);
        var averagePerGame = pinksBought * 1.0 / matches.Count;

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["pinki_show"], pinksBought, $"{averagePerGame:F2}"));
      }
      else
      {
        _client.SendMessage(message.Channel, Globals.Locale["pinki_show_empty"]);
      }
      
      return true;
    }
  }
}
