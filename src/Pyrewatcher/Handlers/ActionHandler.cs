using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Actions;

namespace Pyrewatcher.Handlers
{
  public class ActionHandler
  {
    private readonly Dictionary<string, IAction> _actions = new();
    private readonly IHost _host;
    private readonly ILogger<ActionHandler> _logger;

    public ActionHandler(IHost host, ILogger<ActionHandler> logger)
    {
      _host = host;
      _logger = logger;

      foreach (var actionType in Globals.ActionTypes)
      {
        var action = (IAction) _host.Services.GetService(actionType);
        _actions.Add(action.MsgId, (IAction) _host.Services.GetService(actionType));
      }
    }

    public async Task HandleActionAsync(Dictionary<string, string> action)
    {
      if (_actions.ContainsKey(action["msg-id"]))
      {
        _logger.LogInformation("Action triggered: {action}", action["msg-id"]);
        await _actions[action["msg-id"]].PerformAsync(action);
      }
      else
      {
        _logger.LogInformation("Unknown msg-id: {id}", action["msg-id"]);
      }
    }
  }
}
