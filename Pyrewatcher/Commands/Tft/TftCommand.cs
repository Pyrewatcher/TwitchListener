using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  public class TftCommand : CommandBase<TftCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILogger<TftCommand> _logger;
    private readonly IRiotAccountsRepository _riotAccounts;
    private readonly TftMatchRepository _tftMatches;
    private readonly Utilities _utilities;

    public TftCommand(TwitchClient client, ILogger<TftCommand> logger, IRiotAccountsRepository riotAccounts, TftMatchRepository tftMatches,
                      Utilities utilities)
    {
      _client = client;
      _logger = logger;
      _riotAccounts = riotAccounts;
      _tftMatches = tftMatches;
      _utilities = utilities;
    }

    public override TftCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new TftCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(TftCommandArguments args, ChatMessage message)
    {
      var beginTime = _utilities.GetBeginTime();
      
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccounts.GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<TftMatch>();

      foreach (var account in accounts)
      {
        var matchesList = (await _tftMatches.FindRangeAsync("AccountId = @AccountId AND Timestamp > @Timestamp",
                                                            new TftMatch { AccountId = account.Id, Timestamp = beginTime })).ToList();
        matches.AddRange(matchesList);
      }

      if (matches.Any())
      {
        matches = matches.OrderBy(x => x.Timestamp).ToList();

        var sb = new StringBuilder();

        foreach (var match in matches)
        {
          sb.Append(match.Place);
          sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["tft_show"], sb));
      }
      else
      {
        _client.SendMessage(message.Channel, Globals.Locale["tft_show_empty"]);
      }
      
      return true;
    }
  }
}
