using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Commands;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Handlers
{
  public class CommandHandler
  {
    private readonly ILogger<CommandHandler> _logger;

    private readonly IAliasesRepository _aliasesRepository;
    private readonly ICommandsRepository _commandsRepository;
    private readonly ILatestCommandExecutionsRepository _latestCommandExecutionsRepository;
    private readonly IUsersRepository _usersRepository;

    private readonly CommandHelpers _commandHelpers;
    private readonly TemplateCommandHandler _templateCommandHandler;

    private readonly IDictionary<string, ICommand> _commandClasses;

    public CommandHandler(IHost host, ILogger<CommandHandler> logger, IAliasesRepository aliasesRepository, ICommandsRepository commandsRepository,
                          ILatestCommandExecutionsRepository latestCommandExecutionsRepository, IUsersRepository usersRepository,
                          CommandHelpers commandHelpers, TemplateCommandHandler templateCommandHandler)
    {
      _logger = logger;
      _aliasesRepository = aliasesRepository;
      _commandsRepository = commandsRepository;
      _latestCommandExecutionsRepository = latestCommandExecutionsRepository;
      _usersRepository = usersRepository;
      _commandHelpers = commandHelpers;
      _templateCommandHandler = templateCommandHandler;

      _commandClasses = Globals.CommandTypes.ToDictionary(x => x.Name.Remove(x.Name.Length - 7).TrimStart('_').ToLower(),
                                                          x => (ICommand) host.Services.GetService(x));
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
      var aliasCommand = await _aliasesRepository.GetAliasCommandWithNameByBroadcasterIdAsync(commandText, broadcasterId);

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
      var commandData = await _commandsRepository.GetCommandForChannelByName(commandText, chatMessage.Channel);

      if (commandData is null)
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
      var sender = await _usersRepository.GetUserById(userId);

      if (sender is null)
      {
        sender = new User(userId, chatMessage.DisplayName);
        var inserted = await _usersRepository.InsertUser(sender);

        if (inserted)
        {
         //  _logger.LogInformation("User {user} inserted to the database", chatMessage.DisplayName);
        }
        else
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _usersRepository.UpdateNameById(userId, chatMessage.DisplayName);

        if (!updated)
        {
          // TODO: Log failure
        }
      }

      // Check if sender is permitted to use the command - return if not
      if (!await _commandHelpers.IsUserPermitted(sender, commandData))
      {
        _logger.LogInformation("User {user} is not permitted to use \\{command} command - returning", sender.DisplayName, commandData.Name);

        return;
      }

      // Check if command is on cooldown - skip if sender is administrator, return if not and if command is on cooldown
      var latestExecution = await _latestCommandExecutionsRepository.GetLatestExecutionAsync(broadcasterId, commandData.Id);

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
      var incremented = await _commandsRepository.IncrementUsageCountById(commandData.Id);

      if (!incremented)
      {
        // TODO: Log failure
      }

      if (latestExecution == null)
      {
        var inserted = await _latestCommandExecutionsRepository.InsertLatestExecution(broadcasterId, commandData.Id, DateTime.UtcNow);

        if (!inserted)
        {
          // TODO: Log failure
        }
      }
      else
      {
        var updated = await _latestCommandExecutionsRepository.UpdateLatestExecution(broadcasterId, commandData.Id, DateTime.UtcNow);

        if (!updated)
        {
          // TODO: Log failure
        }
      }
      //_logger.LogDebug("Last usage of command \\{command} updated in the database", command.Name);
    }
  }
}
