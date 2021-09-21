using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class RunyCommand : CommandBase<RunyCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly DatabaseHelpers _databaseHelpers;
    private readonly ILogger<RunyCommand> _logger;
    private readonly IRiotAccountsRepository _riotAccounts;
    private readonly RiotLolApiHelper _riotLolApiHelper;

    public RunyCommand(TwitchClient client, ILogger<RunyCommand> logger, IRiotAccountsRepository riotAccounts, RiotLolApiHelper riotLolApiHelper,
                       DatabaseHelpers databaseHelpers)
    {
      _client = client;
      _logger = logger;
      _riotAccounts = riotAccounts;
      _riotLolApiHelper = riotLolApiHelper;
      _databaseHelpers = databaseHelpers;
    }

    public override RunyCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      return new RunyCommandArguments();
    }

    public override async Task<bool> ExecuteAsync(RunyCommandArguments args, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccounts.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcasterId);

      (var gameInfo, var activeAccount) = await _riotLolApiHelper.SpectatorGetOneByRiotAccountModelsList(accounts.ToList());

      if (gameInfo is null)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["runy_response_noactivegame"], message.Channel));
      }
      else
      {
        Globals.Runes ??= await _databaseHelpers.LoadRunes();

        var streamer = gameInfo.Participants.Find(x => x.SummonerId == activeAccount.SummonerId);
        var runesList = streamer.Perks.PerkIds.Select(x => Globals.Runes[x]).ToList();

        var sb = new StringBuilder();

        sb.Append(Globals.Runes[streamer.Perks.PerkStyle].ToUpper());
        sb.Append(" - ");
        sb.Append(string.Join(", ", runesList.GetRange(0, 4)));
        sb.Append(" | ");
        sb.Append(Globals.Runes[streamer.Perks.PerkSubStyle].ToUpper());
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
