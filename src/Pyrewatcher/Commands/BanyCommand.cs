using System;
using System.Collections.Generic;
using System.Linq;
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
  public class BanyCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly ILolChampionsRepository _lolChampionsRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;

    private readonly IRiotClient _riotClient;

    public BanyCommand(TwitchClient client, ILolChampionsRepository lolChampionsRepository, IRiotAccountsRepository riotAccountsRepository,
                       IRiotClient riotClient)
    {
      _client = client;
      _lolChampionsRepository = lolChampionsRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _riotClient = riotClient;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      foreach (var account in accounts)
      {
        var match = await _riotClient.SpectatorV4.GetActiveGameBySummonerId(account.SummonerId, Enum.Parse<Server>(account.ServerCode));

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

        Globals.LolChampions ??= await _lolChampionsRepository.GetAllAsync();

        var allyBans = match.BannedChampions.Where(x => x.TeamId == broadcaster.TeamId)
                            .OrderBy(x => x.OrderingKey)
                            .Select(x => Globals.LolChampions.ContainsKey(x.ChampionId) ? Globals.LolChampions[x.ChampionId] : "Unknown");

        var enemyBans = match.BannedChampions.Where(x => x.TeamId != broadcaster.TeamId)
                             .OrderBy(x => x.OrderingKey)
                             .Select(x => Globals.LolChampions.ContainsKey(x.ChampionId) ? Globals.LolChampions[x.ChampionId] : "Unknown");

        var response = $"{string.Join(", ", allyBans)} ➔ {string.Join(", ", enemyBans)}";

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response"], response));

        return true;
      }

      // No active game
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response_noactivegame"], message.Channel));

      return true;
    }
  }
}
