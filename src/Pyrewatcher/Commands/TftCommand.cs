using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class TftCommand : ICommand
  {
    private readonly TwitchClient _client;
    
    private readonly ITftMatchesRepository _tftMatchesRepository;

    public TftCommand(TwitchClient client, ITftMatchesRepository tftMatchesRepository)
    {
      _client = client;
      _tftMatchesRepository = tftMatchesRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var matches = (await _tftMatchesRepository.NewGetTodaysMatchesByChannelId(broadcasterId)).ToList();

      if (matches.Any())
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["tft_show"], string.Join(", ", matches.Select(x => x.Place))));
      }
      else
      {
        _client.SendMessage(message.Channel, Globals.Locale["tft_show_empty"]);
      }
      
      return true;
    }
  }
}
