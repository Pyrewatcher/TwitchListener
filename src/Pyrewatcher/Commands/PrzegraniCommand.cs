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
  public class PrzegraniCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly IAdoZRepository _adozRepository;

    public PrzegraniCommand(TwitchClient client, IAdoZRepository adozRepository)
    {
      _client = client;
      _adozRepository = adozRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var entries = (await _adozRepository.GetAllEntriesAsync()).ToList();

      var championsWithLosses = entries
                                  .Where(x => !x.GameWon)
                                  .Select(x => x.ChampionName)
                                  .OrderBy(x => x)
                                  .ToList();

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["przegrani_response"], string.Join(", ", championsWithLosses)));

      return true;
    }
  }
}
