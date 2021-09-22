using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LocaleCommand : CommandBase<LocaleCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly ILocalizationRepository _localization;
    private readonly ILogger<LocaleCommand> _logger;

    public LocaleCommand(TwitchClient client, ILogger<LocaleCommand> logger, ILocalizationRepository localization)
    {
      _client = client;
      _logger = logger;
      _localization = localization;
    }

    public override LocaleCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Locale code not provided - returning");

        return null;
      }

      var args = new LocaleCommandArguments {LocaleCode = argsList[0].ToUpper()};

      return args;
    }

    public override async Task<bool> ExecuteAsync(LocaleCommandArguments args, ChatMessage message)
    {
      if (args.LocaleCode != "PL" && args.LocaleCode != "EN")
      {
        _logger.LogInformation("Invalid locale code: {code} - returning", args.LocaleCode);

        return false;
      }

      Globals.LocaleCode = args.LocaleCode;
      Globals.Locale = await _localization.GetLocalizationByCode(args.LocaleCode);
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["locale_changed"], message.DisplayName));

      return true;
    }
  }
}
