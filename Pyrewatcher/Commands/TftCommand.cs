using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  public class TftCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly IRiotAccountsRepository _riotAccountsRepository;
    private readonly TftMatchRepository _tftMatchesRepository;

    private readonly Utilities _utilities;

    public TftCommand(TwitchClient client, IRiotAccountsRepository riotAccountsRepository, TftMatchRepository tftMatchesRepository,
                      Utilities utilities)
    {
      _client = client;
      _riotAccountsRepository = riotAccountsRepository;
      _tftMatchesRepository = tftMatchesRepository;
      _utilities = utilities;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var beginTime = _utilities.GetBeginTime();
      
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      var matches = new List<TftMatch>();

      foreach (var account in accounts)
      {
        var matchesList = (await _tftMatchesRepository.FindRangeAsync("AccountId = @AccountId AND Timestamp > @Timestamp",
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
