using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ConnectCommandArguments
  {
    public string Channel { get; set; }
  }

  [UsedImplicitly]
  public class ConnectCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<ConnectCommand> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly IUsersRepository _usersRepository;

    private readonly CommandHelpers _commandHelpers;
    private readonly TwitchApiHelper _twitchApiHelper;

    public ConnectCommand(TwitchClient client, IConfiguration config, ILogger<ConnectCommand> logger, IBroadcastersRepository broadcastersRepository,
                          IUsersRepository usersRepository, CommandHelpers commandHelpers, TwitchApiHelper twitchApiHelper)
    {
      _client = client;
      _config = config;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _usersRepository = usersRepository;
      _commandHelpers = commandHelpers;
      _twitchApiHelper = twitchApiHelper;
    }

    private ConnectCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Channel not provided - returning");

        return null;
      }

      var args = new ConnectCommandArguments {Channel = argsList[0]};

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }

      // get broadcaster
      var broadcaster = await _broadcastersRepository.GetByNameAsync(args.Channel);

      if (broadcaster is null)
      {
        var user = await _usersRepository.GetUserByName(args.Channel);

        if (user is null)
        {
          user = await _twitchApiHelper.GetUserByName(args.Channel);

          if (user.Id is 0 or -1)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["connect_error"], message.DisplayName, args.Channel));
            _logger.LogInformation("Broadcaster {broadcaster} couldn't be retrieved - returning", args.Channel);

            return false;
          }

          var userInserted = await _usersRepository.InsertUser(user);

          if (!userInserted)
          {
            // TODO: Message failure
            return false;
          }
        }

        var broadcasterInserted = await _broadcastersRepository.InsertAsync(user.Id);

        if (broadcasterInserted)
        {
          broadcaster = await _broadcastersRepository.GetByNameAsync(args.Channel);
        }
        else
        {
          // TODO: Message failure
          return false;
        }
      }

      // check if already connected
      if (broadcaster.Connected)
      {
        _logger.LogInformation("Pyrewatcher is already connected to channel {channel} - returning", broadcaster.DisplayName);

        return false;
      }

      // connect to broadcaster
      _client.JoinChannel(broadcaster.Name);
      broadcaster.Connected = true;
      var updated = await _broadcastersRepository.ToggleConnectedByIdAsync(broadcaster.Id);

      if (updated)
      {
        // perform tasks
        if (broadcaster.Name != _config.GetSection("Twitch")["Username"].ToLower())
        {
          new Task(async () =>
          {
            await _commandHelpers.UpdateLolMatchDataForBroadcaster(broadcaster);
            await _commandHelpers.UpdateTftMatchDataForBroadcaster(broadcaster);
            await _commandHelpers.UpdateChattersForBroadcaster(broadcaster);
            await _commandHelpers.UpdateLolRankDataForBroadcaster(broadcaster);
            await _commandHelpers.UpdateTftRankDataForBroadcaster(broadcaster);
          }).Start();
        }

        // send message
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["connect_connected"], message.DisplayName, broadcaster.DisplayName));
      }
      else
      {
        // TODO: Message failure
      }

      return true;
    }
  }
}
