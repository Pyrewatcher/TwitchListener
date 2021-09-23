using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
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

    private readonly BroadcasterRepository _broadcastersRepository;

    private readonly CommandHelpers _commandHelpers;
    private readonly DatabaseHelpers _databaseHelpers;

    public ConnectCommand(TwitchClient client, IConfiguration config, ILogger<ConnectCommand> logger, BroadcasterRepository broadcastersRepository,
                          CommandHelpers commandHelpers, DatabaseHelpers databaseHelpers)
    {
      _client = client;
      _config = config;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _commandHelpers = commandHelpers;
      _databaseHelpers = databaseHelpers;
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
      var broadcaster = await _databaseHelpers.GetBroadcaster(args.Channel);

      // check if retrieved
      if (broadcaster == null)
      {
        _logger.LogInformation("Broadcaster {broadcaster} couldn't be retrieved - returning", args.Channel);

        return false;
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
      await _broadcastersRepository.UpdateAsync(broadcaster);

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

      return true;
    }
  }
}
