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
  public class ReloadCommand : CommandBase<ReloadCommandArguments>
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly CommandHelpers _commandHelpers;
    private readonly DatabaseHelpers _databaseHelpers;
    private readonly ILogger<ReloadCommand> _logger;

    public ReloadCommand(TwitchClient client, ILogger<ReloadCommand> logger, CommandHelpers commandHelpers, DatabaseHelpers databaseHelpers,
                         BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _commandHelpers = commandHelpers;
      _databaseHelpers = databaseHelpers;
      _broadcasters = broadcasters;
    }

    public override ReloadCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Component not provided - returning");

        return null;
      }

      var args = new ReloadCommandArguments {Component = argsList[0].ToLower()};

      return args;
    }

    public override async Task<bool> ExecuteAsync(ReloadCommandArguments args, ChatMessage message)
    {
      List<Broadcaster> broadcasters;

      switch (args.Component)
      {
        case "locale":
          Globals.Locale = await _databaseHelpers.LoadLocale(Globals.LocaleCode);
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["reload_locale"], message.DisplayName));

          return true;
        case "ranks" or "ranga":
          broadcasters = (await _broadcasters.FindWithNameAllConnectedAsync()).ToList();
          await _commandHelpers.UpdateLolRankDataForBroadcasters(broadcasters);
          await _commandHelpers.UpdateTftRankDataForBroadcasters(broadcasters);
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["reload_ranks"], message.DisplayName));

          return true;
        case "matches" or "lol" or "kda" or "tft":
          broadcasters = (await _broadcasters.FindWithNameAllConnectedAsync()).ToList();
          await _commandHelpers.UpdateLolMatchDataForBroadcasters(broadcasters);
          await _commandHelpers.UpdateTftMatchDataForBroadcasters(broadcasters);
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["reload_matches"], message.DisplayName));

          return true;
        default:
          _logger.LogInformation("Component {component} is invalid - returning", args.Component);

          return false;
      }
    }
  }
}
