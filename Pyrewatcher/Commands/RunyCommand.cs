using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
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

    private readonly RiotLolApiHelper _riotLolApiHelper;

    public RunyCommand(TwitchClient client, ILolRunesRepository lolRunesRepository, IRiotAccountsRepository riotAccountsRepository,
                       RiotLolApiHelper riotLolApiHelper)
    {
      _client = client;
      _lolRunesRepository = lolRunesRepository;
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
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["runy_response_noactivegame"], message.Channel));
      }
      else
      {
        Globals.LolRunes ??= await _lolRunesRepository.GetAllAsync();

        var streamer = gameInfo.Participants.Find(x => x.SummonerId == activeAccount.SummonerId);
        var runesList = streamer.Perks.PerkIds.Select(x => Globals.LolRunes[x]).ToList();

        var sb = new StringBuilder();

        sb.Append(Globals.LolRunes[streamer.Perks.PerkStyle].ToUpper());
        sb.Append(" - ");
        sb.Append(string.Join(", ", runesList.GetRange(0, 4)));
        sb.Append(" | ");
        sb.Append(Globals.LolRunes[streamer.Perks.PerkSubStyle].ToUpper());
        sb.Append(" - ");
        sb.Append(string.Join(", ", runesList.GetRange(4, 2)));
        sb.Append(" | ");
        sb.Append(string.Join(", ", runesList.GetRange(6, 3)));

        _client.SendMessage(message.Channel, string.Format(Globals.Locale["runy_response"], sb));
      }
      
      return true;
    }
  }
}
