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
  public class TftCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly IRiotAccountsRepository _riotAccountsRepository;
    private readonly ITftMatchesRepository _tftMatchesRepository;

    public TftCommand(TwitchClient client, IRiotAccountsRepository riotAccountsRepository, ITftMatchesRepository tftMatchesRepository)
    {
      _client = client;
      _riotAccountsRepository = riotAccountsRepository;
      _tftMatchesRepository = tftMatchesRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<TftMatch>();

      foreach (var account in accounts)
      {
        var accountMatches = await _tftMatchesRepository.GetTodaysMatchesByAccountId(account.Id);
        matches.AddRange(accountMatches);
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
