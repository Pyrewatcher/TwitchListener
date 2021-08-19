using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class LocaleCommand : CommandBase<LocaleCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly DatabaseHelpers _databaseHelpers;
    private readonly LocaleRepository _locales;
    private readonly ILogger<LocaleCommand> _logger;

    public LocaleCommand(TwitchClient client, ILogger<LocaleCommand> logger, LocaleRepository locales, DatabaseHelpers databaseHelpers)
    {
      _client = client;
      _logger = logger;
      _locales = locales;
      _databaseHelpers = databaseHelpers;
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
      if (await _locales.FindAsync("Code = @Code", new Locale {Code = args.LocaleCode}) == null)
      {
        _logger.LogInformation("Invalid locale code: {code} - returning", args.LocaleCode);

        return false;
      }

      Globals.LocaleCode = args.LocaleCode;
      Globals.Locale = await _databaseHelpers.LoadLocale(Globals.LocaleCode);
      _client.SendMessage(message.Channel, string.Format(Globals.Locale["locale_changed"], message.DisplayName));

      return true;
    }
  }
}
