using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class _8ballCommandArguments
  {
    public string Question { get; set; }
  }

  [UsedImplicitly]
  public class _8ballCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<_8ballCommand> _logger;

    public _8ballCommand(TwitchClient client, ILogger<_8ballCommand> logger)
    {
      _client = client;
      _logger = logger;
    }

    private _8ballCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (!argsList.Any())
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["8ball_usage"], message.DisplayName));
        _logger.LogInformation("Question not provided - returning");

        return null;
      }

      var args = new _8ballCommandArguments {Question = string.Join(' ', argsList)};

      return args;
    }

    public Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return Task.FromResult(false);
      }
      
      var responseAmount = Globals.Locale.Count(x => x.Key.StartsWith("8ball_r_"));

      var responseNumber = Math.Abs(args.Question.GetHashCode()) % responseAmount;

      _client.SendMessage(message.Channel,
                          string.Format(Globals.Locale["8ball_response"], message.DisplayName, Globals.Locale[$"8ball_{responseNumber}"]));

      return Task.FromResult(true);
    }
  }
}
