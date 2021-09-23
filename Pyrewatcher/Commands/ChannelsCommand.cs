using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class ChannelsCommand : ICommand
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<ChannelsCommand> _logger;

    public ChannelsCommand(TwitchClient client, ILogger<ChannelsCommand> logger, IConfiguration config, BroadcasterRepository broadcasters)
    {
      _client = client;
      _logger = logger;
      _config = config;
      _broadcasters = broadcasters;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var channels = (await _broadcasters.FindWithNameAllConnectedAsync()).Where(x => x.Name != _config.GetSection("Twitch")["Username"].ToLower())
                                                                          .Select(x => x.DisplayName)
                                                                          .OrderBy(x => x)
                                                                          .ToList();

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["channels_response"], string.Join(", ", channels)));

      return true;
    }
  }
}
