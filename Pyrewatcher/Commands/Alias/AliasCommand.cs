using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class AliasCommand : CommandBase<AliasCommandArguments>
  {
    private readonly AliasRepository _aliases;
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly ILogger<AliasCommand> _logger;

    public AliasCommand(TwitchClient client, ILogger<AliasCommand> logger, BroadcasterRepository broadcasters, AliasRepository aliases,
                        CommandRepository commands)
    {
      _client = client;
      _logger = logger;
      _broadcasters = broadcasters;
      _aliases = aliases;
      _commands = commands;
    }

    public override AliasCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Action not provided - returning");

        return null;
      }

      var args = new AliasCommandArguments {Action = argsList[0].ToLower()};

      switch (args.Action)
      {
        case "lookup": // <Command> [Broadcaster]
          if (argsList.Count < 2)
          {
            _logger.LogInformation("Command not provided - returning");

            return null;
          }
          else
          {
            args.Command = argsList[1];

            if (argsList.Count != 2)
            {
              args.Broadcaster = argsList[2];
            }
          }

          break;
        case "lookupglobal": // <Command>
          if (argsList.Count < 2)
          {
            _logger.LogInformation("Command not provided - returning");

            return null;
          }
          else
          {
            args.Command = argsList[1];
          }

          break;
        case "create" or "createglobal": // <Alias> <Command>
          if (argsList.Count < 3)
          {
            switch (argsList.Count)
            {
              // missing Alias
              case 1:
                _logger.LogInformation("Alias not provided - returning");

                break;
              // missing Command
              case 2:
                _logger.LogInformation("Command not provided - returning");

                break;
            }

            return null;
          }
          else
          {
            args.Alias = argsList[1];
            args.Command = argsList[2];
          }

          break;
        case "delete": // <Alias>
          if (argsList.Count < 2)
          {
            _logger.LogInformation("Alias not provided - returning");

            return null;
          }
          else
          {
            args.Alias = argsList[1];
          }

          break;
        default:
          _logger.LogInformation("Invalid action: {action} - returning", args.Action);

          return null;
      }

      return args;
    }

    public override async Task<bool> ExecuteAsync(AliasCommandArguments args, ChatMessage message)
    {
      Broadcaster broadcaster;
      List<string> aliasesList;
      Alias alias;

      switch (args.Action)
      {
        case "lookup": // \alias lookup <Command> [Broadcaster]
          // check if Broadcaster is null
          if (args.Broadcaster != null)
          {
            // get given broadcaster
            broadcaster = await _broadcasters.FindWithNameByNameAsync(args.Broadcaster.ToLower());

            // throw an error if broadcaster is not in the database - no need to check if it exists
            if (broadcaster == null)
            {
              _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

              return false;
            }
          }
          else
          {
            // get current broadcaster
            broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);
          }

          // get aliases list
          aliasesList = (await _aliases.FindRangeAsync("NewName = @NewName AND BroadcasterId = @BroadcasterId",
                                                       new Alias {NewName = args.Command.TrimStart('\\'), BroadcasterId = broadcaster.Id}))
                       .Select(x => x.Name)
                       .OrderBy(x => x)
                       .ToList();

          // check if empty
          if (aliasesList.Count == 0)
          {
            // send message alias_lookupchannel_empty
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["alias_lookupchannel_empty"], message.DisplayName, args.Command,
                                              broadcaster.DisplayName));
          }
          else
          {
            // append non-! aliases with \
            for (var i = 0; i < aliasesList.Count; i++)
            {
              aliasesList[i] = aliasesList[i].StartsWith('!') ? aliasesList[i] : $"{'\\'}{aliasesList[i]}";
            }

            // send message alias_lookupchannel
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["alias_lookupchannel"], message.DisplayName, args.Command, broadcaster.DisplayName,
                                              string.Join(", ", aliasesList)));
          }

          break;
        case "lookupglobal": // \account lookupglobal <Command>
          // get aliases list
          aliasesList =
            (await _aliases.FindRangeAsync("NewName = @NewName AND BroadcasterId = 0", new Alias {NewName = args.Command.TrimStart('\\')}))
           .Select(x => x.Name)
           .OrderBy(x => x)
           .ToList();

          // check if empty
          if (aliasesList.Count == 0)
          {
            // send message alias_lookupglobal_empty
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["alias_lookupglobal_empty"], message.DisplayName, args.Command));
          }
          else
          {
            // append non-! aliases with \
            for (var i = 0; i < aliasesList.Count; i++)
            {
              aliasesList[i] = aliasesList[i].StartsWith('!') ? aliasesList[i] : $"{'\\'}{aliasesList[i]}";
            }

            // send message alias_lookupchannel
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["alias_lookupglobal"], message.DisplayName, args.Command,
                                              string.Join(", ", aliasesList)));
          }

          break;
        case "create": // \alias create <Alias> <Command>
          broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);

          // check if a channel or global alias already exists in the database
          if (await _aliases.FindAsync("Name = @Name AND (BroadcasterId = 0 OR BroadcasterId = @BroadcasterId)",
                                       new Alias {Name = args.Alias, BroadcasterId = broadcaster.Id}) != null)
          {
            _logger.LogInformation(
              "Alias \"{alias}\" already exists for broadcaster \"{broadcaster}\" or there is a global alias with that name - returning", args.Alias,
              broadcaster.DisplayName);

            return false;
          }

          // if alias does not start with '!', check commands as well
          if (!args.Alias.StartsWith('!'))
          {
            if (await _commands.FindAsync("Name = @Name AND (Channel = '' OR Channel = @Channel)",
                                          new Command {Name = args.Alias, Channel = broadcaster.Name}) != null)
            {
              _logger.LogInformation("A command with name \"{name}\" already exists and is available for broadcaster \"{broadcaster}\" - returning",
                                     args.Alias, broadcaster.DisplayName);

              return false;
            }
          }

          // create the alias and add it to database
          alias = new Alias {Name = args.Alias, NewName = args.Command, BroadcasterId = broadcaster.Id};
          await _aliases.InsertAsync(alias);

          // send the message
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["alias_create"], message.DisplayName, args.Alias, args.Command, broadcaster.DisplayName));

          break;
        case "createglobal": // \alias createglobal <Alias> <Command>
          // check if alias with given alias name already exists in the database
          if (await _aliases.FindAsync("Name = @Name", new Alias {Name = args.Alias}) != null)
          {
            _logger.LogInformation("Alias \"{alias}\" already exists in the database - returning", args.Alias);

            return false;
          }

          // if alias does not start with '!', check commands as well
          if (!args.Alias.StartsWith('!'))
          {
            if (await _commands.FindAsync("Name = @Name", new Command {Name = args.Alias}) != null)
            {
              _logger.LogInformation("A command with name \"{name}\" already exists in the database - returning", args.Alias);

              return false;
            }
          }

          // create the alias and add it to database
          alias = new Alias {Name = args.Alias, NewName = args.Command, BroadcasterId = 0};
          await _aliases.InsertAsync(alias);

          // send the message
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["alias_createglobal"], message.DisplayName, args.Alias, args.Command));

          break;
        case "delete": // \alias delete <Alias>
          broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);

          // retrieve the alias
          alias = await _aliases.FindAsync("Name = @Name AND (BroadcasterId = 0 OR BroadcasterId = @BroadcasterId)",
                                           new Alias {Name = args.Alias, BroadcasterId = broadcaster.Id});

          if (alias == null)
          {
            _logger.LogInformation(
              "Alias \"{alias}\" does not exist for broadcaster \"{broadcaster}\" and there is no global alias with that name - returning",
              args.Alias, broadcaster.DisplayName);

            return false;
          }

          // delete the alias
          await _aliases.DeleteAsync("Name = @Name AND (BroadcasterId = 0 OR BroadcasterId = @BroadcasterId)", alias);

          // send the message
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["alias_delete"], message.DisplayName, args.Alias));

          break;
      }

      return true;
    }
  }
}
