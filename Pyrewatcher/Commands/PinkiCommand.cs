using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class PinkiCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<PinkiCommand> _logger;
    private readonly LolMatchRepository _lolMatches;
    private readonly IRiotAccountsRepository _riotAccounts;
    private readonly Utilities _utilities;

    public PinkiCommand(TwitchClient client, ILogger<PinkiCommand> logger, IRiotAccountsRepository riotAccounts, LolMatchRepository lolMatches,
                        Utilities utilities)
    {
      _client = client;
      _logger = logger;
      _riotAccounts = riotAccounts;
      _lolMatches = lolMatches;
      _utilities = utilities;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var beginTime = _utilities.GetBeginTime();

      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccounts.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<LolMatch>();

      foreach (var account in accounts)
      {
        var matchesList = (await _lolMatches.FindRangeAsync("AccountId = @AccountId AND Timestamp > @Timestamp AND GameDuration >= @GameDuration",
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
