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
  public class UserCommandArguments
  {
    public string User { get; set; }
  }

  [UsedImplicitly]
  public class UserCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<UserCommand> _logger;

    private readonly IBansRepository _bansRepository;

    private readonly CommandHelpers _commandHelpers;

    public UserCommand(TwitchClient client, ILogger<UserCommand> logger, IBansRepository bansRepository, CommandHelpers commandHelpers)
    {
      _client = client;
      _logger = logger;
      _bansRepository = bansRepository;
      _commandHelpers = commandHelpers;
    }

    private UserCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("User not provided - returning");

        return null;
      }

      var args = new UserCommandArguments {User = argsList[0]};

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

      _client.SendMessage(message.Channel,
                          await _bansRepository.IsUserBannedByIdAsync(user.Id)
                            ? string.Format(Globals.Locale["user_banned"], message.DisplayName, user.DisplayName)
                            : string.Format(Globals.Locale[$"user_is{user.Role.ToLower()}"], message.DisplayName, user.DisplayName));

      return true;
    }
  }
}
