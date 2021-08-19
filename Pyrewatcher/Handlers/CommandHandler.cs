using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Handlers
{
  public class CommandHandler
  {
    private readonly AliasRepository _aliases;
    private readonly Dictionary<string, dynamic> _commandClasses = new();
    private readonly CommandHelpers _commandHelpers;
    private readonly CommandRepository _commands;
    private readonly IHost _host;
    private readonly LatestCommandExecutionRepository _latestCommandExecutions;
    private readonly ILogger<CommandHandler> _logger;
    private readonly TemplateCommandHandler _templateCommandHandler;
    private readonly UserRepository _users;

    public CommandHandler(IHost host, ILogger<CommandHandler> logger, TemplateCommandHandler templateCommandHandler, AliasRepository aliases,
                          CommandRepository commands, CommandHelpers commandHelpers, UserRepository users,
                          LatestCommandExecutionRepository latestCommandExecutions)
    {
      _host = host;
      _logger = logger;
      _templateCommandHandler = templateCommandHandler;
      _aliases = aliases;
      _commands = commands;
      _commandHelpers = commandHelpers;
      _users = users;
      _latestCommandExecutions = latestCommandExecutions;

      foreach (var commandType in Globals.CommandTypes)
      {
        var commandName = commandType.Name.Remove(commandType.Name.Length - 7).TrimStart('_').ToLower();
        _commandClasses.Add(commandName, _host.Services.GetService(commandType));
      }
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
      var alias = await _aliases.FindAsync("Name = @Name AND (BroadcasterId = 0 OR BroadcasterId = @BroadcasterId)",
                                           new Alias {Name = commandText, BroadcasterId = broadcasterId});

      if (alias != null)
      {
        commandText = alias.NewName.ToLower();
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
      var commandExecution = await _latestCommandExecutions.FindAsync("[BroadcasterId] = @BroadcasterId AND [CommandId] = @CommandId",
                                                                      new LatestCommandExecution
                                                                      {
                                                                        BroadcasterId = broadcasterId, CommandId = commandData.Id
                                                                      });

      if (!sender.IsAdministrator)
      {
        if (commandExecution != null)
        {
          var lastUsage = DateTime.Now - DateTime.Parse(commandExecution.LatestExecution);
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
        var commandClass = _commandClasses[commandData.Name];
        executionResult = await commandClass.HandleAsync(command.ArgumentsAsList, chatMessage);
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

      if (commandExecution == null)
      {
        await _latestCommandExecutions.InsertAsync(new LatestCommandExecution
        {
          BroadcasterId = broadcasterId,
          CommandId = commandData.Id,
          LatestExecution = DateTime.UtcNow.ToString("O")
        });
      }
      else
      {
        commandExecution.LatestExecution = DateTime.UtcNow.ToString("O");
        await _latestCommandExecutions.UpdateAsync(commandExecution);
      }
      //_logger.LogDebug("Last usage of command \\{command} updated in the database", command.Name);
    }
  }
}
