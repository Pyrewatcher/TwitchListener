using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LocaleCommandArguments
  {
    public string LocaleCode { get; set; }
  }

  [UsedImplicitly]
  public class LocaleCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<LocaleCommand> _logger;

    private readonly ILocalizationRepository _localizationRepository;

    public LocaleCommand(TwitchClient client, ILogger<LocaleCommand> logger, ILocalizationRepository localizationRepository)
    {
      _client = client;
      _logger = logger;
      _localizationRepository = localizationRepository;
    }

    private LocaleCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Locale code not provided - returning");

        return null;
      }

      var args = new LocaleCommandArguments {LocaleCode = argsList[0].ToUpper()};

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      if (args.LocaleCode != "PL" && args.LocaleCode != "EN")
      {
        _logger.LogInformation("Invalid locale code: {code} - returning", args.LocaleCode);

        return false;
      }

      Globals.LocaleCode = args.LocaleCode;
      Globals.Locale = await _localizationRepository.GetLocalizationByCodeAsync(args.LocaleCode);
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["locale_changed"], message.DisplayName));

      return true;
    }
  }
}
