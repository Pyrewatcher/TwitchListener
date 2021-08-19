using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;

namespace Pyrewatcher.Handlers
{
  public class TemplateCommandHandler
  {
    private readonly TwitchClient _client;
    private readonly CommandVariableRepository _commandVariables;
    private readonly ILogger<TemplateCommandHandler> _logger;

    public TemplateCommandHandler(TwitchClient client, ILogger<TemplateCommandHandler> logger, CommandVariableRepository commandVariables)
    {
      _client = client;
      _logger = logger;
      _commandVariables = commandVariables;
    }

    public async Task<bool> HandleTextAsync(string broadcasterName, Command textCommand)
    {
      var textVariable = await _commandVariables.FindAsync("CommandId = @CommandId AND Name = @Name",
                                                           new CommandVariable {CommandId = textCommand.Id, Name = "text"});
      _client.SendMessage(broadcasterName, textVariable.Value);

      return true;
    }
  }
}
