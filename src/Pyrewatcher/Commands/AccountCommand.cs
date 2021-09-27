using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Utilities;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class AccountCommandArguments
  {
    public string Action { get; set; }
    public string Broadcaster { get; set; }
    public Game Game { get; set; }
    public Server Server { get; set; }
    public string SummonerName { get; set; }
    public string AccountKey { get; set; }
    public string NewDisplayName { get; set; }
  }

  [UsedImplicitly]
  public class AccountCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<AccountCommand> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;
    
    private readonly IRiotClient _riotClient;

    public AccountCommand(TwitchClient client, ILogger<AccountCommand> logger, IBroadcastersRepository broadcastersRepository,
                          IRiotAccountsRepository riotAccountsRepository, IRiotClient riotClient)
    {
      _client = client;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _riotClient = riotClient;
    }

    private AccountCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_usage"], message.DisplayName));
        _logger.LogInformation("Action not provided - returning");

        return null;
      }

      var args = new AccountCommandArguments {Action = argsList[0].ToLower()};

      if (args.Action is "list" or "inactive")
      {
        if (argsList.Count > 1)
        {
          args.Broadcaster = argsList[1];
        }
      }
      else if (args.Action is "lookup" or "remove" or "toggle" or "update")
      {
        if (argsList.Count < 2)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale[$"account_{args.Action}_usage"], message.DisplayName));
          _logger.LogInformation("Account key not provided - returning");

          return null;
        }
        else
        {
          args.AccountKey = argsList[1].TrimStart('[').TrimEnd(']');
        }
      }
      else if (args.Action == "add")
      {
        if (argsList.Count < 4)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_usage"], message.DisplayName));

          switch (argsList.Count)
          {
            // missing Game
            case 1:
              _logger.LogInformation("Game not provided - returning");

              break;
            // missing Server
            case 2:
              _logger.LogInformation("Server not provided - returning");

              break;
            // missing SummonerName
            case 3:
              _logger.LogInformation("Summoner name not provided - returning");

              break;
          }

          return null;
        }
        else
        {
          var game = argsList[1].ToGameEnum();

          if (game is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidGame"], message.DisplayName, argsList[1]));
            _logger.LogInformation("Invalid game - returning");

            return null;
          }

          var server = argsList[2].ToServerEnum();

          if (server is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidServer"], message.DisplayName, argsList[2]));
            _logger.LogInformation("Invalid server - returning");

            return null;
          }

          args.Game = game.Value;
          args.Server = server.Value;
          args.SummonerName = string.Join(' ', argsList.Skip(3));
        }
      }
      else if (args.Action == "display")
      {
        if (argsList.Count < 3)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_display_usage"], message.DisplayName));

          switch (argsList.Count)
          {
            // missing AccountKey
            case 1:
              _logger.LogInformation("Account key not provided - returning");

              break;
            // missing NewDisplayName
            case 2:
              _logger.LogInformation("New display name not provided - returning");

              break;
          }

          return null;
        }
        else
        {
          args.AccountKey = argsList[1];
          args.NewDisplayName = string.Join(' ', argsList.Skip(2));
        }
      }
      else
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_usage"], message.DisplayName));
        _logger.LogInformation("Invalid action: {action} - returning", args.Action);

        return null;
      }

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList, message);

      if (args is null)
      {
        return false;
      }

      var channelId = long.Parse(message.RoomId);

      if (args.Action == "list")
      {
        Broadcaster broadcaster;

        // check if Broadcaster is null
        if (args.Broadcaster is not null)
        {
          // get given broadcaster
          broadcaster = await _broadcastersRepository.GetByNameAsync(args.Broadcaster);

          // throw an error if broadcaster is not in the database - no need to check if it exists
          if (broadcaster is null)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_broadcasterDoesNotExist"], message.DisplayName, args.Broadcaster));
            _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

            return false;
          }
        }
        else
        {
          // get current broadcaster
          broadcaster = await _broadcastersRepository.GetByNameAsync(message.Channel);
        }

        // get accounts list
        var accounts = (await _riotAccountsRepository.NewGetActiveAccountsForDisplayByChannelIdAsync(broadcaster.Id)).ToList();

        // check if there are any accounts
        if (accounts.Any())
        {
          // send message account_list
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_list"], message.DisplayName, broadcaster.DisplayName, string.Join(" | ", accounts)));
        }
        else
        {
          // send message account_list_empty
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_list_empty"], message.DisplayName, broadcaster.DisplayName));
        }
      }
      else if (args.Action == "inactive")
      {
        Broadcaster broadcaster;

        // check if Broadcaster is null
        if (args.Broadcaster is not null)
        {
          // get given broadcaster
          broadcaster = await _broadcastersRepository.GetByNameAsync(args.Broadcaster);

          // throw an error if broadcaster is not in the database - no need to check if it exists
          if (broadcaster is null)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_broadcasterDoesNotExist"], message.DisplayName, args.Broadcaster));
            _logger.LogInformation("Broadcaster {broadcaster} doesn't exist in the database - returning", args.Broadcaster);

            return false;
          }
        }
        else
        {
          // get current broadcaster
          broadcaster = await _broadcastersRepository.GetByNameAsync(message.Channel);
        }

        // get accounts list
        var accounts = (await _riotAccountsRepository.NewGetInactiveAccountsForDisplayByChannelIdAsync(broadcaster.Id)).ToList();

        // check if there are any accounts
        if (accounts.Any())
        {
          // send message account_inactive
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_inactive"], message.DisplayName, broadcaster.DisplayName, string.Join(" | ", accounts)));
        }
        else
        {
          // send message account_inactive_empty
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_inactive_empty"], message.DisplayName, broadcaster.DisplayName));
        }
      }
      else if (args.Action == "lookup")
      {
        // retrieve account
        var account = await _riotAccountsRepository.NewGetChannelAccountForLookupByKeyAsync(channelId, args.AccountKey);

        if (account is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountKey));
          _logger.LogInformation("Account with key {key} is not assigned to this channel - returning", args.AccountKey);

          return false;
        }

        // send message account_lookup_response
        _client.SendMessage(message.Channel,
                            string.Format(Globals.Locale["account_lookup_response"],
                                          message.DisplayName,
                                          account.DisplayName,
                                          account.Game.ToFullName(),
                                          account.Server,
                                          account.SummonerName,
                                          Globals.Locale[account.Active ? "yes" : "no"],
                                          account.DisplayableRank ?? Globals.Locale["ranga_value_unavailable"]));
      }
      else if (args.Action == "add")
      {
        // check if account game is already assigned to channel
        if (await _riotAccountsRepository.NewIsAccountGameAssignedToChannel(channelId, args.Game, args.Server, args.SummonerName))
        {
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_add_accountAlreadyExists"], message.DisplayName,
                                            $"{args.Game.ToAbbreviation()} {args.Server} {args.SummonerName}", message.Channel));
          _logger.LogInformation("Account \"{account}\" is already assigned to channel {channel} - returning",
                                 $"{args.Game.ToAbbreviation()} {args.Server} {args.SummonerName}", message.Channel);

          return false;
        }

        string summonerName;
        // check if account is already in the database
        if (!await _riotAccountsRepository.NewExistsAccountGame(args.Game, args.Server, args.SummonerName))
        {
          // if not, get account data from Riot API and insert it
          var summoner = args.Game switch
          {
            Game.LeagueOfLegends => await _riotClient.SummonerV4.GetSummonerByName(args.SummonerName, args.Server),
            Game.TeamfightTactics => await _riotClient.TftSummonerV1.GetSummonerByName(args.SummonerName, args.Server),
            _ => default(ISummonerDto)
          };

          if (summoner is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
            _logger.LogInformation("Loading account data failed - returning");

            return false;
          }

          summonerName = summoner.SummonerName;

          var inserted = await _riotAccountsRepository.NewInsertAccountGameFromDto(args.Game, args.Server, summoner);

          if (!inserted)
          {
            // TODO: Message failure
            return false;
          }
        }
        else
        {
          summonerName = await _riotAccountsRepository.NewGetAccountSummonerName(args.Server, RiotUtilities.NormalizeSummonerName(args.SummonerName));
        }

        // assign account to channel
        var accountKey = RiotUtilities.GenerateAccountKey();
        var assigned = await _riotAccountsRepository.NewAssignAccountGameToChannel(channelId, args.Game, args.Server, summonerName, accountKey);

        if (assigned)
        {
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_add_accountAdded"], message.DisplayName,
                                            $"{args.Game.ToAbbreviation()} {args.Server} {summonerName}"));
        }
        else
        {
          // TODO: Message failure
          return false;
        }
      }
      else if (args.Action == "remove")
      {
        // check if account with given key exists
        var account = await _riotAccountsRepository.NewGetChannelAccountForLookupByKeyAsync(channelId, args.AccountKey);

        if (account is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountKey));
          _logger.LogInformation("Account with key {key} is not assigned to this channel - returning", args.AccountKey);

          return false;
        }

        var deleted = await _riotAccountsRepository.NewDeleteChannelAccountByKey(args.AccountKey);

        if (deleted)
        {
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_remove_accountRemoved"], message.DisplayName, account.DisplayName));
        }
        else
        {
          // TODO: Message failure
          return false;
        }
      }
      else if (args.Action == "toggle")
      {
        // check if account with given key exists
        var account = await _riotAccountsRepository.NewGetChannelAccountForLookupByKeyAsync(channelId, args.AccountKey);

        if (account is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountKey));
          _logger.LogInformation("Account with key {key} is not assigned to this channel - returning", args.AccountKey);

          return false;
        }

        var toggled = await _riotAccountsRepository.NewToggleActiveByKey(args.AccountKey);

        if (toggled)
        {
          account.Active = !account.Active;
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_toggle_toggled"], message.DisplayName, account.DisplayName,
                                            account.Active ? Globals.Locale["account_value_active"] : Globals.Locale["account_value_inactive"]));
        }
        else
        {
          // TODO: Message failure
          return false;
        }
      }
      else if (args.Action == "update")
      {
        // check if account with given key exists
        var account = await _riotAccountsRepository.NewGetChannelAccountForLookupByKeyAsync(channelId, args.AccountKey);

        if (account is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountKey));
          _logger.LogInformation("Account with key {key} is not assigned to this channel - returning", args.AccountKey);

          return false;
        }

        var summoner = account.Game switch
        {
          Game.LeagueOfLegends => await _riotClient.SummonerV4.GetSummonerByName(account.SummonerName, account.Server),
          Game.TeamfightTactics => await _riotClient.TftSummonerV1.GetSummonerByName(account.SummonerName, account.Server),
          _ => default(ISummonerDto)
        };

        if (summoner is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
          _logger.LogInformation("Loading account data failed - returning");

          return false;
        }
        else if (summoner.SummonerName != account.SummonerName)
        {
          var updated = await _riotAccountsRepository.NewUpdateSummonerNameByKeyAsync(args.AccountKey, summoner.SummonerName);

          if (updated)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_update_updated"], message.DisplayName, account.SummonerName, summoner.SummonerName));
          }
          else
          {
            // TODO: Message failure
            return false;
          }
        }
        else
        {
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_update_noChange"], message.DisplayName, account.DisplayName));
          _logger.LogInformation("Summoner name for account {account} didn't change - returning", account.DisplayName);
        }
      }
      else if (args.Action == "display")
      {
        // check if account with given key exists
        var account = await _riotAccountsRepository.NewGetChannelAccountForLookupByKeyAsync(channelId, args.AccountKey);

        if (account is null)
        {
          _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountKey));
          _logger.LogInformation("Account with key {key} is not assigned to this channel - returning", args.AccountKey);

          return false;
        }

        var newDisplayName = args.NewDisplayName == "-"
          ? $"{account.Game.ToAbbreviation()} {account.Server} {account.SummonerName}"
          : args.NewDisplayName;

        var updated = await _riotAccountsRepository.NewUpdateDisplayNameByKeyAsync(args.AccountKey, newDisplayName);

        if (updated)
        {
          if (args.NewDisplayName == "-")
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_display_cleared"], message.DisplayName, newDisplayName));
          }
          else
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_display_changed"], message.DisplayName,
                                              $"{account.Game.ToAbbreviation()} {account.Server} {account.SummonerName}", args.NewDisplayName));
          }
        }
        else
        {
          // TODO: Message failure
        }
      }

      return true;
    }
  }
}
