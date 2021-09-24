using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class PinkiCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly LolMatchRepository _lolMatchesRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;

    private readonly Utilities _utilities;

    public PinkiCommand(TwitchClient client, LolMatchRepository lolMatchesRepository, IRiotAccountsRepository riotAccountsRepository,
                        Utilities utilities)
    {
      _client = client;
      _lolMatchesRepository = lolMatchesRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _utilities = utilities;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var beginTime = _utilities.GetBeginTime();

      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<LolMatch>();

      foreach (var account in accounts)
      {
        var matchesList = (await _lolMatchesRepository.FindRangeAsync("AccountId = @AccountId AND Timestamp > @Timestamp AND GameDuration >= @GameDuration",
                                                            new LolMatch { AccountId = account.Id, Timestamp = beginTime, GameDuration = 330 })).ToList();
        matches.AddRange(matchesList);
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
