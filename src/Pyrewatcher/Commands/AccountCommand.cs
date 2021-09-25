using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class AccountCommandArguments
  {
    public string Action { get; set; }
    public string Broadcaster { get; set; }
    public string Game { get; set; }
    public string Server { get; set; }
    public string SummonerName { get; set; }
    public long AccountId { get; set; }
    public string NewDisplayName { get; set; }
  }

  [UsedImplicitly]
  public class AccountCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<AccountCommand> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;
    
    private readonly Utilities _utilities;
    private readonly IRiotClient _riotClient;

    public AccountCommand(TwitchClient client, ILogger<AccountCommand> logger, IBroadcastersRepository broadcastersRepository,
                          IRiotAccountsRepository riotAccountsRepository, Utilities utilities, IRiotClient riotClient)
    {
      _client = client;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _utilities = utilities;
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

      switch (args.Action)
      {
        case "list" or "listactive": // [Broadcaster]
          if (argsList.Count > 1)
          {
            args.Broadcaster = argsList[1];
          }

          break;
        case "add": // <Game> <Server> <SummonerName>
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
            args.Game = argsList[1];
            args.Server = argsList[2];
            args.SummonerName = string.Join(' ', argsList.Skip(3));
          }

          break;
        case "remove" or "toggleactive" or "update": // <AccountId>
          if (argsList.Count < 2)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale[$"account_{args.Action}_usage"], message.DisplayName));
            _logger.LogInformation("Account ID not provided - returning");

            return null;
          }
          else
          {
            if (!long.TryParse(argsList[1], out var accountId))
            {
              _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_invalidId"], message.DisplayName, argsList[1]));
              _logger.LogInformation("Provided ID is invalid: {id} - returning", argsList[1]);

              return null;
            }

            args.AccountId = accountId;
          }

          break;
        case "display": // <AccountId> <NewDisplayName>
          if (argsList.Count < 3)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_display_usage"], message.DisplayName));

            switch (argsList.Count)
            {
              // missing AccountId
              case 1:
                _logger.LogInformation("Account ID not provided - returning");

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
            if (!long.TryParse(argsList[1], out var accountId))
            {
              _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_invalidId"], message.DisplayName, argsList[1]));
              _logger.LogInformation("Provided ID is invalid: {id} - returning", argsList[1]);

              return null;
            }

            args.AccountId = accountId;
            args.NewDisplayName = string.Join(' ', argsList.Skip(2));
          }

          break;
        default:
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

      List<RiotAccount> accountsList;
      Broadcaster broadcaster;
      RiotAccount account;

      switch (args.Action)
      {
        case "list": // \account list [Broadcaster]
        {
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
          accountsList = (await _riotAccountsRepository.GetAccountsByBroadcasterIdAsync(broadcaster.Id)).ToList();

          // check if empty
          if (accountsList.Count == 0)
          {
            // send message account_list_empty
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_list_empty"], message.DisplayName, broadcaster.DisplayName));
          }
          else
          {
            // send message account_list
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_list"], message.DisplayName, broadcaster.DisplayName,
                                              ListToStringListAction(accountsList)));
          }

          break;
        }
        case "listactive": // \account listactive [Broadcaster]
        {
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
          accountsList = (await _riotAccountsRepository.GetActiveAccountsByBroadcasterIdAsync(broadcaster.Id)).ToList();

          // check if empty
          if (accountsList.Count == 0)
          {
            // send message account_listactive_empty
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_listactive_empty"], message.DisplayName, broadcaster.DisplayName));
          }
          else
          {
            // send message account_listactive
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_listactive"], message.DisplayName, broadcaster.DisplayName,
                                              ListToStringListactiveAction(accountsList)));
          }

          break;
        }
        case "add": // \account add <Game> <Server> <SummonerName>
        {
          // check if game abbreviation exists
          if (_utilities.GetGameFullName(args.Game.ToLower()) is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidGame"], message.DisplayName, args.Game));
            _logger.LogInformation("Game {game} doesn't exist in the database - returning", args.Game);

            return false;
          }

          // check if server exists
          var serverApiCode = _utilities.GetServerApiCode(args.Server.ToUpper());

          if (serverApiCode is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidServer"], message.DisplayName, args.Server));
            _logger.LogInformation("Server {server} doesn't exist in the database - returning", args.Server);

            return false;
          }

          // check if account already exists in channel's Riot account list
          broadcaster = await _broadcastersRepository.GetByNameAsync(message.Channel);
          account = await _riotAccountsRepository.GetAccountForDisplayByDetailsAsync(args.Game, args.Server, args.SummonerName, broadcaster.Id);

          if (account is not null)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_add_accountAlreadyExists"], message.DisplayName, account.ToStringShort()));
            _logger.LogInformation("Account \"{account}\" already exists in the database - returning", account.ToStringShort());

            return false;
          }

          // get data about the account from Riot API
          switch (args.Game.ToLower())
          {
            case "lol":
              var lolData = await _riotClient.SummonerV4.GetSummonerByName(args.SummonerName, Enum.Parse<Server>(args.Server, true));
              account = lolData is null
                ? null
                : new RiotAccount(broadcaster.Id, args.Game, lolData.Name, args.Server, lolData.SummonerId, lolData.AccountId, lolData.Puuid);

              break;
            case "tft":
              var tftData = await _riotClient.TftSummonerV1.GetSummonerByName(args.SummonerName, Enum.Parse<Server>(args.Server, true));
              account = tftData is null
                ? null
                : new RiotAccount(broadcaster.Id, args.Game, tftData.Name, args.Server, tftData.SummonerId, tftData.AccountId, tftData.Puuid);

              break;
          }

          if (account is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
            _logger.LogInformation("Loading account data failed - returning");

            return false;
          }

          await _riotAccountsRepository.InsertAccount(account);
          
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_add_accountAdded"], message.DisplayName, account.ToStringList()));

          break;
        }
        case "remove": // \account remove <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccountsRepository.GetAccountForDisplayByIdAsync(args.AccountId);

          if (account is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          // delete the account
          var deleted = await _riotAccountsRepository.DeleteByIdAsync(args.AccountId);

          if (deleted)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_remove_accountRemoved"], message.DisplayName, account.ToStringShort()));
          }
          else
          {
            // TODO: Message failure
          }

          break;
        }
        case "toggleactive": // \account toggleactive <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccountsRepository.GetAccountForDisplayByIdAsync(args.AccountId);

          if (account is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }
          
          var toggled = await _riotAccountsRepository.ToggleActiveByIdAsync(args.AccountId);

          if (toggled)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_toggleactive_toggled"], message.DisplayName, account.ToStringShort(),
                                              account.Active ? Globals.Locale["account_value_active"] : Globals.Locale["account_value_inactive"]));
          }
          else
          {
            // TODO: Message failure
          }

          break;
        }
        case "display": // \account display <AccountId> <NewDisplayName>
        {
          // check if account with given id exists
          account = await _riotAccountsRepository.GetAccountForDisplayByIdAsync(args.AccountId);

          if (account is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          var newDisplayName = args.NewDisplayName == "-" ? "" : args.NewDisplayName;
          var updated = await _riotAccountsRepository.UpdateDisplayNameByIdAsync(args.AccountId, newDisplayName);

          if (updated)
          {
            if (args.NewDisplayName == "-")
            {
              _client.SendMessage(message.Channel,
                                  string.Format(Globals.Locale["account_display_cleared"], message.DisplayName, account.ToStringShort()));
            }
            else
            {
              _client.SendMessage(message.Channel,
                                  string.Format(Globals.Locale["account_display_changed"], message.DisplayName, account.ToStringShort(),
                                                args.NewDisplayName));
            }
          }
          else
          {
            // TODO: Message failure
          }

          break;
        }
        case "update": // \account update <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccountsRepository.GetAccountForApiCallsByIdAsync(args.AccountId);

          if (account is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          string requestedSummonerName = null;
          // get data about the account from Riot API
          switch (account.GameAbbreviation.ToLower())
          {
            case "lol":
              var lolData = await _riotClient.SummonerV4.GetSummonerByPuuid(account.Puuid, Enum.Parse<Server>(account.ServerCode, true));
              requestedSummonerName = lolData?.Name;

              break;
            case "tft":
              var tftData = await _riotClient.TftSummonerV1.GetSummonerByPuuid(account.Puuid, Enum.Parse<Server>(account.ServerCode, true));
              requestedSummonerName = tftData?.Name;

              break;
          }

          if (requestedSummonerName is null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
            _logger.LogInformation("Loading account data failed - returning");

            return false;
          }
          else if (requestedSummonerName != account.SummonerName)
          {
            var updated = await _riotAccountsRepository.UpdateSummonerNameByIdAsync(args.AccountId, requestedSummonerName);

            if (updated)
            {
              _client.SendMessage(message.Channel,
                                  string.Format(Globals.Locale["account_update_updated"], message.DisplayName, account.SummonerName, requestedSummonerName));
            }
            else
            {
              // TODO: Message failure
            }
          }
          else
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_update_noChange"], message.DisplayName, account.ToStringShort()));
            _logger.LogInformation("Summoner name for account {account} didn't change - returning", account.ToStringShort());

            return false;
          }

          break;
        }
      }

      return true;
    }

    private static string ListToStringListAction(List<RiotAccount> list)
    {
      var sb = new StringBuilder();

      foreach (var account in list)
      {
        sb.Append(account.ToStringList());
        sb.Append("; ");
      }

      sb.Remove(sb.Length - 2, 2);

      return sb.ToString();
    }

    private static string ListToStringListactiveAction(List<RiotAccount> list)
    {
      var sb = new StringBuilder();

      foreach (var account in list)
      {
        sb.Append(account.ToStringListactive());
        sb.Append("; ");
      }

      sb.Remove(sb.Length - 2, 2);

      return sb.ToString();
    }
  }
}
