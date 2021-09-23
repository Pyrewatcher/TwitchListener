using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

  [UsedImplicitly]
  public class DisconnectCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<DisconnectCommand> _logger;

    private readonly BroadcasterRepository _broadcastersRepository;

    public DisconnectCommand(TwitchClient client, IConfiguration config, ILogger<DisconnectCommand> logger,
                             BroadcasterRepository broadcastersRepository)
    {
      _client = client;
      _config = config;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
    }

    private DisconnectCommandArguments ParseAndValidateArguments(List<string> argsList)
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
      var args = ParseAndValidateArguments(argsList);

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
      var broadcaster = await _broadcastersRepository.FindWithNameByNameAsync(channel);

      // check if already disconnected
      if (broadcaster == null || !broadcaster.Connected)
      {
        _logger.LogInformation("Pyrewatcher is not connected to channel {channel} - returning", channel);

        return false;
      }

      // disconnect from broadcaster
      _client.LeaveChannel(broadcaster.Name);
      broadcaster.Connected = false;
      await _broadcastersRepository.UpdateAsync(broadcaster);

      // send message
      if (broadcaster.Name != message.Channel)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["disconnect_disconnected"], message.DisplayName, broadcaster.DisplayName));
      }

      return true;
    }
  }
}
