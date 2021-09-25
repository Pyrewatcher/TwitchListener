using System.Threading.Tasks;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using TwitchLib.Client;

namespace Pyrewatcher.Handlers
{
  public class TemplateCommandHandler
  {
    private readonly TwitchClient _client;

    private readonly ICommandVariablesRepository _commandVariablesRepository;

    public TemplateCommandHandler(TwitchClient client, ICommandVariablesRepository commandVariablesRepository)
    {
      _client = client;
      _commandVariablesRepository = commandVariablesRepository;
    }

    public async Task<bool> HandleTextAsync(string broadcasterName, Command textCommand)
    {
      var text = await _commandVariablesRepository.GetCommandTextById(textCommand.Id);

      _client.SendMessage(broadcasterName, text);

      return true;
    }
  }
}
