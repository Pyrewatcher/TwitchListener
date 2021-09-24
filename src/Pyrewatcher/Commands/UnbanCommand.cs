using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class UnbanCommandArguments
  {
    public string User { get; set; }
  }

  [UsedImplicitly]
  public class UnbanCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<UnbanCommand> _logger;

    private readonly IBansRepository _bansRepository;

    private readonly CommandHelpers _commandHelpers;

    public UnbanCommand(TwitchClient client, ILogger<UnbanCommand> logger, IBansRepository bansRepository, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _bansRepository = bansRepository;
      _commandHelpers = commandHelpers;
    }

    private UnbanCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("User not provided - returning");

        return null;
      }

      var args = new UnbanCommandArguments {User = argsList[0]};

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      var user = await _commandHelpers.GetUser(args.User);

      if (user == null)
      {
        _logger.LogInformation("User {user} couldn't be found or does not exist - returning", args.User);

        return false;
      }

      if (user.IsAdministrator)
      {
        _logger.LogInformation("Cannot unban {user} because they're an Administrator - returning", args.User);

        return false;
      }
      
      if (!await _bansRepository.IsUserBannedByIdAsync(user.Id))
      {
        _logger.LogInformation("User {user} already isn't banned - returning", args.User);

        return false;
      }

      var unbanned = await _bansRepository.UnbanUserByIdAsync(user.Id);

      if (unbanned)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["unban_unbanned"], message.DisplayName, user.DisplayName));
      }
      else
      {
        // TODO: Message failure
      }

      return true;
    }
  }
}
