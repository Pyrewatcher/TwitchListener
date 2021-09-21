using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class RangaCommand : CommandBase<RangaCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILogger<RangaCommand> _logger;
    private readonly IRiotAccountsRepository _riotAccounts;

    public RangaCommand(TwitchClient client, ILogger<RangaCommand> logger, IRiotAccountsRepository riotAccounts)
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
      var accounts = await _riotAccounts.GetActiveAccountsWithRankByBroadcasterIdAsync(broadcasterId);

      if (accounts.Any())
      {
        var displayableAccounts = new List<string>();

        foreach (var account in accounts)
        {
          var displayableAccountBuilder = new StringBuilder(account.DisplayName == "" ? account.ToStringShort() : account.DisplayName);
          displayableAccountBuilder.Append(": ");
          displayableAccountBuilder.Append(account.DisplayableRank ?? Globals.Locale["ranga_value_unavailable"]);
          displayableAccountBuilder.Append(" ➔ ");
          displayableAccountBuilder.Append(GenerateAccountUrl(account));

          displayableAccounts.Add(displayableAccountBuilder.ToString());
        }

        _client.SendMessage(message.Channel, string.Join(" | ", displayableAccounts));
      }
      else
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ranga_noaccounts"], message.Channel));
      }

      return true;
    }

    private static string GenerateAccountUrl(RiotAccount account)
    {
      if (account is null)
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
