using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class UnbanCommand : CommandBase<UnbanCommandArguments>
  {
    private readonly IBansRepository _bans;
    private readonly TwitchClient _client;
    private readonly CommandHelpers _commandHelpers;
    private readonly ILogger<UnbanCommand> _logger;

    public UnbanCommand(TwitchClient client, ILogger<UnbanCommand> logger, IBansRepository bans, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _bans = bans;
      _commandHelpers = commandHelpers;
    }

    public override UnbanCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("User not provided - returning");

        return null;
      }

      var args = new UnbanCommandArguments {User = argsList[0]};

      return args;
    }

    public override async Task<bool> ExecuteAsync(UnbanCommandArguments args, ChatMessage message)
    {
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
      
      if (!await _bans.IsUserBannedByIdAsync(user.Id))
      {
        _logger.LogInformation("User {user} already isn't banned - returning", args.User);

        return false;
      }

      var unbanned = await _bans.UnbanUserByIdAsync(user.Id);

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
