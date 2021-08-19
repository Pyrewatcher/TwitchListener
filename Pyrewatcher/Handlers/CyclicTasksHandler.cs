using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.Helpers;

namespace Pyrewatcher.Handlers
{
  public class CyclicTasksHandler
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly CommandHelpers _commandHelpers;
    private readonly ILogger<CyclicTasksHandler> _logger;
    private readonly List<Task> _tasks = new();

    public CyclicTasksHandler(ILogger<CyclicTasksHandler> logger, CommandHelpers commandHelpers, BroadcasterRepository broadcasters)
    {
      _logger = logger;
      _commandHelpers = commandHelpers;
      _broadcasters = broadcasters;
      CreateTasks();
    }

    private void CreateTasks()
    {
      _logger.LogInformation("abc");
      _tasks.Add(new Task(async () =>
      {
        while (true)
        {
          var broadcasters = (await _broadcasters.FindWithNameAllConnectedAsync()).ToList();
          await _commandHelpers.UpdateLolMatchDataForBroadcasters(broadcasters);
          await Task.Delay(TimeSpan.FromSeconds(1));
          await _commandHelpers.UpdateTftMatchDataForBroadcasters(broadcasters);
          await Task.Delay(TimeSpan.FromSeconds(1));
          await _commandHelpers.UpdateChattersForBroadcasters(broadcasters);
          await Task.Delay(TimeSpan.FromSeconds(1));
          await _commandHelpers.UpdateLolRankDataForBroadcasters(broadcasters);
          await Task.Delay(TimeSpan.FromSeconds(1));
          await _commandHelpers.UpdateTftRankDataForBroadcasters(broadcasters);
          await Task.Delay(TimeSpan.FromMinutes(2));
        }
      }));
    }

    public void RunTasks()
    {
      _logger.LogInformation("def");
      foreach (var task in _tasks)
      {
        task.Start();
      }
    }
  }
}
