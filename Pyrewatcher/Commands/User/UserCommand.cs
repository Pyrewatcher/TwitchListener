﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class UserCommand : CommandBase<UserCommandArguments>
  {
    private readonly IBansRepository _bans;
    private readonly TwitchClient _client;
    private readonly CommandHelpers _commandHelpers;
    private readonly ILogger<UserCommand> _logger;

    public UserCommand(TwitchClient client, ILogger<UserCommand> logger, IBansRepository bans, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _bans = bans;
      _commandHelpers = commandHelpers;
    }

    public override UserCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("User not provided - returning");

        return null;
      }

      var args = new UserCommandArguments {User = argsList[0]};

      return args;
    }

    public override async Task<bool> ExecuteAsync(UserCommandArguments args, ChatMessage message)
    {
      var user = await _commandHelpers.GetUser(args.User);

      if (user == null)
      {
        _logger.LogInformation("User {user} couldn't be found or does not exist - returning", args.User);

        return false;
      }

      _client.SendMessage(message.Channel,
                          await _bans.IsUserBannedByIdAsync(user.Id)
                            ? string.Format(Globals.Locale["user_banned"], message.DisplayName, user.DisplayName)
                            : string.Format(Globals.Locale[$"user_is{user.Role.ToLower()}"], message.DisplayName, user.DisplayName));

      return true;
    }
  }
}
