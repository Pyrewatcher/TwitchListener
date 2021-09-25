using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class RunyCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly ILolRunesRepository _lolRunesRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;

    private readonly IRiotClient _riotClient;

    public RunyCommand(TwitchClient client, ILolRunesRepository lolRunesRepository, IRiotAccountsRepository riotAccountsRepository,
                       IRiotClient riotClient)
    {
      _client = client;
      _lolRunesRepository = lolRunesRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _riotClient = riotClient;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      foreach (var account in accounts)
      {
        var match = await _riotClient.SpectatorV4.GetActiveGameBySummonerId(account.SummonerId, Enum.Parse<Server>(account.ServerCode, true));

        if (match is null)
        {
          continue;
        }

        var broadcaster = match.Players.FirstOrDefault(x => x.SummonerId == account.SummonerId);

        if (broadcaster is null)
        {
          // TODO: Message failure
          return true;
        }

        Globals.LolRunes ??= await _lolRunesRepository.GetAllAsync();

        var runes = broadcaster.Runes.RuneIds.Select(x => Globals.LolRunes.ContainsKey(x) ? Globals.LolRunes[x] : "Unknown");

        var sb = new StringBuilder();

        sb.Append(Globals.LolRunes[broadcaster.Runes.PrimaryPathId].ToUpper());
        sb.Append(" - ");
        sb.Append(string.Join(", ", runes.Take(4)));
        sb.Append(" | ");
        sb.Append(Globals.LolRunes[broadcaster.Runes.SecondaryPathId].ToUpper());
        sb.Append(" - ");
        sb.Append(string.Join(", ", runes.Skip(4).Take(2)));
        sb.Append(" | ");
        sb.Append(string.Join(", ", runes.Skip(6)));

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["runy_response"], sb));

        return true;
      }

      // No active game
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["runy_response_noactivegame"], message.Channel));

      return true;
    }
  }
}
