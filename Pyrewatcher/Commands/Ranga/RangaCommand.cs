using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class RangaCommand : CommandBase<RangaCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILogger<RangaCommand> _logger;
    private readonly RiotAccountRepository _riotAccounts;

    public RangaCommand(TwitchClient client, ILogger<RangaCommand> logger, RiotAccountRepository riotAccounts)
    {
      _client = client;
      _logger = logger;
      _riotAccounts = riotAccounts;
    }

    public override RangaCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new RangaCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(RangaCommandArguments args, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);

      var accountsListLol =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcasterId, GameAbbreviation = "lol", Active = true})).ToList();
      var accountsListTft =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcasterId, GameAbbreviation = "tft", Active = true})).ToList();

      if (accountsListLol.Count > 0 || accountsListTft.Count > 0)
      {
        var accountStrings = new List<string>();

        foreach (var account in accountsListLol)
        {
          var entry = new LeagueEntryDto
          {
            Tier = account.Tier,
            Rank = account.Rank,
            LeaguePoints = account.LeaguePoints,
            MiniSeries = new MiniSeriesDto {Progress = account.SeriesProgress}
          };

          accountStrings.Add(account.DisplayName == ""
                               ? $"{account.ToStringShort()}: {entry} ➔ {GenerateAccountUrl(account)}"
                               : $"{account.DisplayName}: {entry} ➔ {GenerateAccountUrl(account)}");
        }

        foreach (var account in accountsListTft)
        {
          var entry = new LeagueEntryDto
          {
            Tier = account.Tier,
            Rank = account.Rank,
            LeaguePoints = account.LeaguePoints,
            MiniSeries = new MiniSeriesDto {Progress = account.SeriesProgress}
          };

          accountStrings.Add(account.DisplayName == ""
                               ? $"{account.ToStringShort()}: {entry} ➔ {GenerateAccountUrl(account)}"
                               : $"{account.DisplayName}: {entry} ➔ {GenerateAccountUrl(account)}");
        }

        _client.SendMessage(message.Channel, string.Join(" | ", accountStrings));
      }
      else
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ranga_noaccounts"], message.Channel));
      }

      return true;
    }

    private static string GenerateAccountUrl(RiotAccount account)
    {
      if (account == null)
      {
        return null;
      }

      var url = account.GameAbbreviation switch
      {
        "lol" => $"https://{account.ServerCode.ToLower()}.op.gg/summoner/userName={account.SummonerName.Replace(" ", "+")}",
        "tft" => $"https://lolchess.gg/profile/{account.ServerCode.ToLower()}/{account.SummonerName.Replace(" ", "")}",
        _ => null
      };

      return EncodeUrl(url);
    }

    private static string EncodeUrl(string url)
    {
      var bytes = Encoding.UTF8.GetBytes(url);
      var urlEncoded = string.Join("", bytes.Select(b => b > 127 ? Uri.HexEscape((char) b) : ((char) b).ToString()));

      return urlEncoded;
    }
  }
}
