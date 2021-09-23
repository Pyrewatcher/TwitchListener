using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ReloadCommandArguments
  {
    public string Component { get; set; }
  }

  [UsedImplicitly]
  public class ReloadCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<ReloadCommand> _logger;

    private readonly BroadcasterRepository _broadcastersRepository;
    private readonly ILocalizationRepository _localizationRepository;

    private readonly CommandHelpers _commandHelpers;

    public ReloadCommand(TwitchClient client, ILogger<ReloadCommand> logger, BroadcasterRepository broadcastersRepository,
                         ILocalizationRepository localizationRepository, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _localizationRepository = localizationRepository;
      _commandHelpers = commandHelpers;
    }

    private ReloadCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Component not provided - returning");

        return null;
      }

      var args = new ReloadCommandArguments {Component = argsList[0].ToLower()};

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      List<Broadcaster> broadcasters;

      switch (args.Component)
      {
        case "locale":
          Globals.Locale = await _localizationRepository.GetLocalizationByCodeAsync(Globals.LocaleCode);
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["reload_locale"], message.DisplayName));

          return true;
        case "ranks" or "ranga":
          broadcasters = (await _broadcastersRepository.FindWithNameAllConnectedAsync()).ToList();
          await _commandHelpers.UpdateLolRankDataForBroadcasters(broadcasters);
          await _commandHelpers.UpdateTftRankDataForBroadcasters(broadcasters);
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["reload_ranks"], message.DisplayName));

          return true;
        case "matches" or "lol" or "kda" or "tft":
          broadcasters = (await _broadcastersRepository.FindWithNameAllConnectedAsync()).ToList();
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
