using System;
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
  public class AdoZCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly IAdoZRepository _adozRepository;

    public AdoZCommand(TwitchClient client, IAdoZRepository adozRepository)
    {
      _client = client;
      _adozRepository = adozRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var entries = (await _adozRepository.GetAllEntriesAsync()).ToList();

      var winrateWins = entries.Count(x => x.GameWon);
      var winrateLosses = entries.Count(x => !x.GameWon);
      var winratePercentage = $"{winrateWins * 100.0 / (winrateWins + winrateLosses):F1}";

      var latestEntry = entries.Last();
      var latestEntryInfo =
        $"{latestEntry.ChampionName} {(latestEntry.GameWon ? "✔" : "✖")} {latestEntry.Kills}/{latestEntry.Deaths}/{latestEntry.Assists}";

      var mostKillsAmount = entries.Max(x => x.Kills);
      var mostKillsEntries = entries.Where(x => x.Kills == mostKillsAmount).ToList();
      var mostKillsChampions = string.Join(", ", mostKillsEntries.Select(x => x.ChampionName));

      var mostDeathsAmount = entries.Max(x => x.Deaths);
      var mostDeathsEntries = entries.Where(x => x.Deaths == mostDeathsAmount).ToList();
      var mostDeathsChampions = string.Join(", ", mostDeathsEntries.Select(x => x.ChampionName));

      var fastestWinSeconds = entries.Where(x => x.GameWon).Min(x => x.Duration);
      var fastestWinEntries = entries.Where(x => x.GameWon && x.Duration == fastestWinSeconds);
      var fastestWinChampions = string.Join(", ", fastestWinEntries.Select(x => x.ChampionName));
      var fastestWinTime = TimeSpan.FromSeconds(fastestWinSeconds).ToString(@"mm\:ss");

      var fastestLossSeconds = entries.Where(x => !x.GameWon).Min(x => x.Duration);
      var fastestLossEntries = entries.Where(x => !x.GameWon && x.Duration == fastestLossSeconds);
      var fastestLossChampions = string.Join(", ", fastestLossEntries.Select(x => x.ChampionName));
      var fastestLossTime = TimeSpan.FromSeconds(fastestLossSeconds).ToString(@"mm\:ss");

      _client.SendMessage(message.Channel, 
                          string.Format(Globals.Locale["adoz_response"],
                                        winratePercentage,
                                        winrateWins, winrateLosses,
                                        latestEntryInfo,
                                        mostKillsAmount, mostKillsChampions,
                                        mostDeathsAmount, mostDeathsChampions,
                                        fastestWinTime, fastestWinChampions,
                                        fastestLossTime, fastestLossChampions));

      return true;
    }
  }
}
