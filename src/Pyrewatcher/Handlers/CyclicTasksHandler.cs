using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Helpers;

namespace Pyrewatcher.Handlers
{
  public class CyclicTasksHandler
  {
    private readonly IBroadcastersRepository _broadcastersRepository;

    private readonly CommandHelpers _commandHelpers;

    private readonly List<Task> _tasks = new();

    public CyclicTasksHandler(IBroadcastersRepository broadcastersRepository, CommandHelpers commandHelpers)
    {
      _broadcastersRepository = broadcastersRepository;
      _commandHelpers = commandHelpers;

      CreateTasks();
    }

    private void CreateTasks()
    {
      _tasks.Add(new Task(async () =>
      {
        while (true)
        {
          var broadcasters = (await _broadcastersRepository.GetConnectedAsync()).ToList();
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
      foreach (var task in _tasks)
      {
        task.Start();
      }
    }
  }
}
