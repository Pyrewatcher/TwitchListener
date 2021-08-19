using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ConnectCommand : CommandBase<ConnectCommandArguments>
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly CommandHelpers _commandHelpers;
    private readonly IConfiguration _configuration;
    private readonly DatabaseHelpers _databaseHelpers;
    private readonly ILogger<ConnectCommand> _logger;

    public ConnectCommand(TwitchClient client, ILogger<ConnectCommand> logger, CommandHelpers commandHelpers, BroadcasterRepository broadcasters,
                          DatabaseHelpers databaseHelpers, IConfiguration configuration)
    {
      _client = client;
      _logger = logger;
      _commandHelpers = commandHelpers;
      _broadcasters = broadcasters;
      _databaseHelpers = databaseHelpers;
      _configuration = configuration;
    }

    public override ConnectCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Channel not provided - returning");

        return null;
      }

      var args = new ConnectCommandArguments {Channel = argsList[0]};

      return args;
    }

    public override async Task<bool> ExecuteAsync(ConnectCommandArguments args, ChatMessage message)
    {
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
      await _broadcasters.UpdateAsync(broadcaster);

      // perform tasks
      if (broadcaster.Name != _configuration.GetSection("Twitch")["Username"].ToLower())
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
