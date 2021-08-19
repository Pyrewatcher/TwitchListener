using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LolCommand : CommandBase<LolCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly DatabaseHelpers _databaseHelpers;
    private readonly ILogger<LolCommand> _logger;
    private readonly LolMatchRepository _lolMatches;
    private readonly RiotAccountRepository _riotAccounts;
    private readonly Utilities _utilities;

    public LolCommand(TwitchClient client, ILogger<LolCommand> logger, RiotAccountRepository riotAccounts, LolMatchRepository lolMatches,
                      Utilities utilities, DatabaseHelpers databaseHelpers)
    {
      _client = client;
      _logger = logger;
      _riotAccounts = riotAccounts;
      _lolMatches = lolMatches;
      _utilities = utilities;
      _databaseHelpers = databaseHelpers;
    }

    public override LolCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new LolCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(LolCommandArguments args, ChatMessage message)
    {
      var beginTime = _utilities.GetBeginTime();

      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = long.Parse(message.RoomId), GameAbbreviation = "lol", Active = true}))
       .ToList();

      var matches = new List<LolMatch>();

      foreach (var account in accountsList)
      {
        var matchesList = (await _lolMatches.FindRangeAsync("AccountId = @AccountId AND Timestamp > @Timestamp AND GameDuration >= 330",
                                                            new LolMatch {AccountId = account.Id, Timestamp = beginTime})).ToList();
        matches.AddRange(matchesList);
      }

      Globals.LolChampions ??= await _databaseHelpers.LoadLolChampions();

      if (matches.Count > 0)
      {
        var wins = matches.Count(x => x.Result == "W");
        var losses = matches.Count(x => x.Result == "L");

        matches = matches.OrderBy(x => x.Timestamp).ToList();

        if (matches.Count > 1)
        {
          var sb = new StringBuilder();

          foreach (var match in matches)
          {
            sb.Append(Globals.LolChampions[match.ChampionId]);
            sb.Append(" ");
            sb.Append(match.Result == "W" ? "✔" : "✖");

            if (matches.Count < 10)
            {
              sb.Append(" ");
              sb.Append(match.Kda);
            }

            sb.Append("; ");
          }

          sb.Remove(sb.Length - 2, 2);

          var kdaList = matches.Select(x => new Kda(x.Kda)).ToList();
          var kdaSum = new Kda("0/0/0");

          foreach (var kda in kdaList)
          {
            kdaSum.Add(kda);
          }

          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["lol_show_more_than_one"], wins, losses, sb, kdaSum.ToStringWithRatio(),
                                            kdaSum.ToStringAverage(matches.Count)));
        }
        else
        {
          var match = matches[0];
          var matchKda = new Kda(match.Kda);
          var matchString = $"{Globals.LolChampions[match.ChampionId]} {(match.Result == "W" ? "✔" : "✖")} {matchKda.ToStringWithRatio()}";

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

      public Kda(string kda)
      {
        var kdaSplit = kda.Split('/');
        _kills = int.Parse(kdaSplit[0]);
        _deaths = int.Parse(kdaSplit[1]);
        _assists = int.Parse(kdaSplit[2]);
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
