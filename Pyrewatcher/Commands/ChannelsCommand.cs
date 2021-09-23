using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class ChannelsCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;

    private readonly BroadcasterRepository _broadcastersRepository;

    public ChannelsCommand(TwitchClient client, IConfiguration config, BroadcasterRepository broadcastersRepository)
    {
      _client = client;
      _config = config;
      _broadcastersRepository = broadcastersRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var channels = (await _broadcastersRepository.FindWithNameAllConnectedAsync()).Where(x => x.Name != _config.GetSection("Twitch")["Username"].ToLower())
                                                                          .Select(x => x.DisplayName)
                                                                          .OrderBy(x => x)
                                                                          .ToList();

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["channels_response"], string.Join(", ", channels)));

      return true;
    }
  }
}
