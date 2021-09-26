using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class LolCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly ILolChampionsRepository _lolChampionsRepository;
    private readonly ILolMatchesRepository _lolMatchesRepository;

    public LolCommand(TwitchClient client, ILolChampionsRepository lolChampionsRepository, ILolMatchesRepository lolMatchesRepository)
    {
      _client = client;
      _lolChampionsRepository = lolChampionsRepository;
      _lolMatchesRepository = lolMatchesRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var matches = (await _lolMatchesRepository.NewGetTodaysMatchesByChannelId(broadcasterId)).ToList();

      Globals.LolChampions ??= await _lolChampionsRepository.GetAllAsync();

      if (matches.Any())
      {
        var wins = matches.Count(x => x.WonMatch);
        var losses = matches.Count - wins;

        if (matches.Count > 1)
        {
          var sb = new StringBuilder();

          var kdaSum = new Kda();

          foreach (var match in matches)
          {
            sb.Append(Globals.LolChampions[match.ChampionId]);
            sb.Append(' ');
            sb.Append(match.WonMatch ? "✔" : "✖");

            var kda = new Kda(match);

            if (matches.Count < 10)
            {
              sb.Append(' ');
              sb.Append(kda);
            }

            sb.Append("; ");

            kdaSum.Add(kda);
          }

          sb.Remove(sb.Length - 2, 2);

          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["lol_show_more_than_one"], wins, losses, sb, kdaSum.ToStringWithRatio(),
                                            kdaSum.ToStringAverage(matches.Count)));
        }
        else
        {
          var match = matches[0];
          var matchKda = new Kda(match);
          var matchString = $"{Globals.LolChampions[match.ChampionId]} {(match.WonMatch ? "✔" : "✖")} {matchKda.ToStringWithRatio()}";

          _client.SendMessage(message.Channel, string.Format(Globals.Locale["lol_show_one"], wins, losses, matchString));
        }
      }
      else
      {
        _client.SendMessage(message.Channel, Globals.Locale["lol_show_empty"]);
      }
      
      return true;
    }

    private struct Kda
    {
      private int _kills;
      private int _deaths;
      private int _assists;

      public Kda(NewLolMatch match)
      {
        _kills = match.Kills;
        _deaths = match.Deaths;
        _assists = match.Assists;
      }

      private double Ratio
      {
        get => _deaths == 0 ? (_kills + _assists) * 1.0 : (_kills + _assists) * 1.0 / _deaths;
      }

      public override string ToString()
      {
        return $"{_kills}/{_deaths}/{_assists}";
      }

      public string ToStringWithRatio()
      {
        return $"{_kills}/{_deaths}/{_assists} ({Ratio:F})";
      }

      public string ToStringAverage(int amountOfGames)
      {
        return $"{_kills * 1.0 / amountOfGames:F}/{_deaths * 1.0 / amountOfGames:F}/{_assists * 1.0 / amountOfGames:F}";
      }

      public void Add(Kda kda)
      {
        _kills += kda._kills;
        _deaths += kda._deaths;
        _assists += kda._assists;
      }
    }
  }
}
