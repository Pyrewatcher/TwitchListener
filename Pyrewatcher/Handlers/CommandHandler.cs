using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Commands;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Handlers
{
  public class CommandHandler
  {
    private readonly IAliasesRepository _aliases;
    private readonly IDictionary<string, ICommand> _commandClasses;
    private readonly CommandHelpers _commandHelpers;
    private readonly CommandRepository _commands;
    private readonly IHost _host;
    private readonly ILatestCommandExecutionsRepository _latestCommandExecutions;
    private readonly ILogger<CommandHandler> _logger;
    private readonly TemplateCommandHandler _templateCommandHandler;
    private readonly UserRepository _users;

    public CommandHandler(IHost host, ILogger<CommandHandler> logger, TemplateCommandHandler templateCommandHandler, IAliasesRepository aliases,
                          CommandRepository commands, CommandHelpers commandHelpers, UserRepository users,
                          ILatestCommandExecutionsRepository latestCommandExecutions)
    {
      _host = host;
      _logger = logger;
      _templateCommandHandler = templateCommandHandler;
      _aliases = aliases;
      _commands = commands;
      _commandHelpers = commandHelpers;
      _users = users;
      _latestCommandExecutions = latestCommandExecutions;

      _commandClasses = Globals.CommandTypes.ToDictionary(x => x.Name.Remove(x.Name.Length - 7).TrimStart('_').ToLower(),
                                                          x => (ICommand) _host.Services.GetService(x));
    }

    public async Task HandleCommand(ChatCommand command)
    {
      var chatMessage = command.ChatMessage;
      _logger.LogInformation("{user} issued command \"{command}\" in channel {channel}", chatMessage.DisplayName, chatMessage.Message,
                             chatMessage.Channel);

      var commandText = $"{command.CommandIdentifier}{command.CommandText}";
      var broadcasterId = long.Parse(chatMessage.RoomId);

      var sw = Stopwatch.StartNew();

      // Check if command is a valid alias for the broadcaster
      var aliasCommand = await _aliases.GetAliasCommandWithNameByBroadcasterIdAsync(commandText, broadcasterId);

      if (aliasCommand is not null)
      {
        commandText = aliasCommand.ToLower();
      }
      else if (command.CommandIdentifier == '!')
      {
        _logger.LogInformation("There is no alias with name \"{name}\" available for broadcaster \"{broadcaster}\" - returning", commandText,
                               chatMessage.Channel);

        return;
      }
      else
      {
        commandText = command.CommandText;
      }

      // Check if command is available for the broadcaster
      var commandData = await _commands.FindAsync("Name = @Name AND (Channel = '' OR Channel = @Channel)",
                                                  new Command {Name = commandText, Channel = chatMessage.Channel});

      if (commandData == null)
      {
        _logger.LogInformation("There is no command with name \"{name}\" available for broadcaster \"{broadcaster}\" - returning", commandText,
                               chatMessage.Channel);

        return;
      }
      else
      {
        //_logger.LogDebug("Command \"{name}\" found for broadcaster \"{broadcaster}\"", command.Name, broadcasterName);
      }

      var userId = long.Parse(chatMessage.UserId);
      // Add sender if sender doesn't exist in the database, update if does
      var sender = await _users.FindAsync("Id = @Id", new User {Id = userId});

      if (sender == null)
      {
        sender = new User {Name = chatMessage.Username, DisplayName = chatMessage.DisplayName, Id = userId};
        await _users.InsertAsync(sender);
        //_logger.LogInformation("User {user} inserted to the database", commandDeserialized["display-name"]);
      }
      else
      {
        sender.DisplayName = chatMessage.DisplayName;
        sender.Name = chatMessage.Username;
        await _users.UpdateAsync(sender);
      }

      // Check if sender is permitted to use the command - return if not
      if (!await _commandHelpers.IsUserPermitted(sender, commandData))
      {
        _logger.LogInformation("User {user} is not permitted to use \\{command} command - returning", sender.DisplayName, commandData.Name);

        return;
      }

      // Check if command is on cooldown - skip if sender is administrator, return if not and if command is on cooldown
      var latestExecution = await _latestCommandExecutions.GetLatestExecutionAsync(broadcasterId, commandData.Id);

      if (!sender.IsAdministrator)
      {
        if (latestExecution is not null)
        {
          var lastUsage = DateTime.UtcNow - latestExecution.Value;
          var cooldown = TimeSpan.FromSeconds(commandData.Cooldown);

          if (lastUsage < cooldown)
          {
            _logger.LogInformation("Command \\{command} is on cooldown ({seconds:F2}s left) - returning", commandData.Name,
                                   (cooldown - lastUsage).TotalMilliseconds / 1000.0);

            return;
          }
        }
      }
      else
      {
        //_logger.LogDebug("User {user} is administrator - cooldown check skipped", sender.DisplayName);
      }

      // Parse command arguments and execute command
      bool executionResult;

      if (commandData.Type == "Text")
      {
        executionResult = await _templateCommandHandler.HandleTextAsync(chatMessage.Channel, commandData);
      }
      else
      {
        executionResult = await _commandClasses[commandData.Name].ExecuteAsync(command.ArgumentsAsList, chatMessage);
      }

      // Check execution result and update command usage if executed successfully
      sw.Stop();

      if (executionResult)
      {
        _logger.LogInformation("Successfully executed \\{command} command. Time: {time} ms", commandData.Name, sw.ElapsedMilliseconds);
        commandData.UsageCount++;
      }
      else
      {
        _logger.LogInformation("Failed execution of \\{command} command. Time: {time} ms", commandData.Name, sw.ElapsedMilliseconds);
      }

      // Update command and latest execution
      await _commands.UpdateAsync(commandData);

      if (latestExecution == null)
      {
        var inserted = await _latestCommandExecutions.InsertLatestExecution(broadcasterId, commandData.Id, DateTime.UtcNow);

        if (!inserted)
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _latestCommandExecutions.UpdateLatestExecution(broadcasterId, commandData.Id, DateTime.UtcNow);

        if (!updated)
        {
          // TODO: Log failure
        }
      }
      //_logger.LogDebug("Last usage of command \\{command} updated in the database", command.Name);
    }
  }
}
