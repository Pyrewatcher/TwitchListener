using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class BanyCommand : CommandBase<BanyCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILogger<BanyCommand> _logger;
    private readonly LolChampionRepository _lolChampions;
    private readonly RiotAccountRepository _riotAccounts;
    private readonly RiotLolApiHelper _riotLolApiHelper;

    public BanyCommand(TwitchClient client, ILogger<BanyCommand> logger, LolChampionRepository lolChampions, RiotAccountRepository riotAccounts,
                       RiotLolApiHelper riotLolApiHelper)
    {
      _client = client;
      _logger = logger;
      _lolChampions = lolChampions;
      _riotAccounts = riotAccounts;
      _riotLolApiHelper = riotLolApiHelper;
    }

    public override BanyCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new BanyCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(BanyCommandArguments args, ChatMessage message)
    {
      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = long.Parse(message.RoomId), GameAbbreviation = "lol", Active = true}))
       .ToList();

      (var gameInfo, var activeAccount) = await _riotLolApiHelper.SpectatorGetOneByRiotAccountModelsList(accountsList);

      if (gameInfo == null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response_noactivegame"], message.Channel));
      }
      else
      {
        var streamer = gameInfo.Participants.Find(x => x.SummonerId == activeAccount.SummonerId);

        if (streamer == null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response_noactivegame"], message.Channel));

          return true;
        }

        var allyBansIds = gameInfo.BannedChampions.Where(x => x.TeamId == streamer.TeamId).OrderBy(x => x.PickTurn).ToList();
        var enemyBansIds = gameInfo.BannedChampions.Except(allyBansIds).OrderBy(x => x.PickTurn).ToList();

        var champions = (await _lolChampions.FindRangeByIdAsync(gameInfo.BannedChampions.Select(x => x.ChampionId))).ToList();

        var allyBans = new List<string>();

        foreach (var bannedChampion in allyBansIds)
        {
          var champion = champions.Find(x => x.Id == bannedChampion.ChampionId)?.Name ?? "Unknown";
          allyBans.Add(champion);
        }

        var enemyBans = new List<string>();

        foreach (var bannedChampion in enemyBansIds)
        {
          var champion = champions.Find(x => x.Id == bannedChampion.ChampionId)?.Name ?? "Unknown";
          enemyBans.Add(champion);
        }

        var response = $"{string.Join(", ", allyBans)} ➔ {string.Join(", ", enemyBans)}";

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["bany_response"], response));
      }

      return true;
    }
  }
}
