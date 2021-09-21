using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class BanCommand : CommandBase<BanCommandArguments>
  {
    private readonly IBansRepository _bans;
    private readonly TwitchClient _client;
    private readonly CommandHelpers _commandHelpers;
    private readonly ILogger<BanCommand> _logger;

    public BanCommand(TwitchClient client, ILogger<BanCommand> logger, IBansRepository bans, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _bans = bans;
      _commandHelpers = commandHelpers;
    }

    public override BanCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("User not provided - returning");

        return null;
      }

      var args = new BanCommandArguments {User = argsList[0]};

      return args;
    }

    public override async Task<bool> ExecuteAsync(BanCommandArguments args, ChatMessage message)
    {
      var user = await _commandHelpers.GetUser(args.User);

      if (user == null)
      {
        _logger.LogInformation("User {user} couldn't be found or does not exist - returning", args.User);

        return false;
      }

      if (user.IsAdministrator)
      {
        _logger.LogInformation("Cannot ban {user} because they're an Administrator - returning", args.User);

        return false;
      }
      
      if (await _bans.IsUserBannedByIdAsync(user.Id))
      {
        _logger.LogInformation("User {user} is already banned - returning", args.User);

        return false;
      }

      var banned = await _bans.BanUserByIdAsync(user.Id);

      if (banned)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ban_banned"], message.DisplayName, user.DisplayName));
      }
      else
      {
        // TODO: Message failure
      }

      return true;
    }
  }
}
