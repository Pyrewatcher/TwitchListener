using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
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

    private readonly RiotLolApiHelper _riotLolApiHelper;

    public BanyCommand(TwitchClient client, ILolChampionsRepository lolChampionsRepository, IRiotAccountsRepository riotAccountsRepository,
                       RiotLolApiHelper riotLolApiHelper)
    {
      _client = client;
      _lolChampionsRepository = lolChampionsRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _riotLolApiHelper = riotLolApiHelper;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      (var gameInfo, var activeAccount) = await _riotLolApiHelper.SpectatorGetOneByRiotAccountModelsList(accounts.ToList());

      if (gameInfo is null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response_noactivegame"], message.Channel));
      }
      else
      {
        var streamer = gameInfo.Participants.Find(x => x.SummonerId == activeAccount.SummonerId);

        if (streamer is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response_noactivegame"], message.Channel));

          return true;
        }

        var allyBansIds = gameInfo.BannedChampions.Where(x => x.TeamId == streamer.TeamId).OrderBy(x => x.PickTurn).ToList();
        var enemyBansIds = gameInfo.BannedChampions.Except(allyBansIds).OrderBy(x => x.PickTurn).ToList();
        
        var allyBans = new List<string>();

        Globals.LolChampions ??= await _lolChampionsRepository.GetAllAsync();

        foreach (var bannedChampion in allyBansIds)
        {
          var champion = Globals.LolChampions.ContainsKey(bannedChampion.ChampionId) ? Globals.LolChampions[bannedChampion.ChampionId] : "Unknown";
          allyBans.Add(champion);
        }

        var enemyBans = new List<string>();

        foreach (var bannedChampion in enemyBansIds)
        {
          var champion = Globals.LolChampions.ContainsKey(bannedChampion.ChampionId) ? Globals.LolChampions[bannedChampion.ChampionId] : "Unknown";
          enemyBans.Add(champion);
        }

        var response = $"{string.Join(", ", allyBans)} ➔ {string.Join(", ", enemyBans)}";

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response"], response));
      }
      
      return true;
    }
  }
}
