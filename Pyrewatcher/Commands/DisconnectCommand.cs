using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class DisconnectCommandArguments
  {
    public string Channel { get; set; }
  }

  public class DisconnectCommand : ICommand
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<DisconnectCommand> _logger;

    public DisconnectCommand(TwitchClient client, ILogger<DisconnectCommand> logger, IConfiguration config, BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _config = config;
      _broadcasters = broadcasters;
    }

    private DisconnectCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      var args = new DisconnectCommandArguments();

      if (argsList.Count > 0)
      {
        args.Channel = argsList[0];
      }

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return false;
      }

      var channel = args.Channel != null ? args.Channel.ToLower() : message.Channel;

      if (channel == _config.GetSection("Twitch")["Username"].ToLower())
      {
        _logger.LogInformation("Cannot disconnect Pyrewatcher from Pyrewatcher's channel - returning");

        return false;
      }

      // get broadcaster
      var broadcaster = await _broadcasters.FindWithNameByNameAsync(channel);

      // check if already disconnected
      if (broadcaster == null || !broadcaster.Connected)
      {
        _logger.LogInformation("Pyrewatcher is not connected to channel {channel} - returning", channel);

        return false;
      }

      // disconnect from broadcaster
      _client.LeaveChannel(broadcaster.Name);
      broadcaster.Connected = false;
      await _broadcasters.UpdateAsync(broadcaster);

      // send message
      if (broadcaster.Name != message.Channel)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["disconnect_disconnected"], message.DisplayName, broadcaster.DisplayName));
      }

      return true;
    }
  }
}
