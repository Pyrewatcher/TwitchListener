using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Helpers;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class AccountCommand : CommandBase<AccountCommandArguments>
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchClient _client;
    private readonly ILogger<AccountCommand> _logger;
    private readonly RiotAccountRepository _riotAccounts;
    private readonly RiotLolApiHelper _riotLolApiHelper;
    private readonly RiotTftApiHelper _riotTftApiHelper;
    private readonly Utilities _utilities;
    private readonly ISummonerV4Client _summonerV4;

    public AccountCommand(TwitchClient client, ILogger<AccountCommand> logger, BroadcasterRepository broadcasters, RiotAccountRepository riotAccounts,
                          RiotLolApiHelper riotLolApiHelper, RiotTftApiHelper riotTftApiHelper, Utilities utilities, ISummonerV4Client summonerV4)
    {
      _client = client;
      _logger = logger;
      _broadcasters = broadcasters;
      _riotAccounts = riotAccounts;
      _riotLolApiHelper = riotLolApiHelper;
      _riotTftApiHelper = riotTftApiHelper;
      _utilities = utilities;
      _summonerV4 = summonerV4;
    }

    public override AccountCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
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

    public override async Task<bool> ExecuteAsync(AccountCommandArguments args, ChatMessage message)
    {
      List<RiotAccount> accountsList;
      Broadcaster broadcaster;
      RiotAccount account;
      SummonerV4Dto data;

      switch (args.Action)
      {
        case "list": // \account list [Broadcaster]
        {
          // check if Broadcaster is null
          if (args.Broadcaster != null)
          {
            // get given broadcaster
            broadcaster = await _broadcasters.FindWithNameByNameAsync(args.Broadcaster.ToLower());

            // throw an error if broadcaster is not in the database - no need to check if it exists
            if (broadcaster == null)
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
            broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);
          }

          // get accounts list
          accountsList = (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId", new RiotAccount {BroadcasterId = broadcaster.Id}))
           .ToList();

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
          if (args.Broadcaster != null)
          {
            // get given broadcaster
            broadcaster = await _broadcasters.FindWithNameByNameAsync(args.Broadcaster);

            // throw an error if broadcaster is not in the database - no need to check if it exists
            if (broadcaster == null)
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
            broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);
          }

          // get accounts list
          accountsList = (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND Active = @Active",
                                                             new RiotAccount {BroadcasterId = broadcaster.Id, Active = true})).ToList();

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
          if (_utilities.GetGameFullName(args.Game.ToLower()) == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidGame"], message.DisplayName, args.Game));
            _logger.LogInformation("Game {game} doesn't exist in the database - returning", args.Game);

            return false;
          }

          // check if server exists
          var serverApiCode = _utilities.GetServerApiCode(args.Server.ToUpper());

          if (serverApiCode == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_add_invalidServer"], message.DisplayName, args.Server));
            _logger.LogInformation("Server {server} doesn't exist in the database - returning", args.Server);

            return false;
          }

          // check if account already exists in channel's Riot account list
          broadcaster = await _broadcasters.FindWithNameByNameAsync(message.Channel);
          account = await _riotAccounts.FindAsync(
            "GameAbbreviation = @GameAbbreviation AND ServerCode = @ServerCode AND NormalizedSummonerName = @NormalizedSummonerName AND BroadcasterId = @BroadcasterId",
            new RiotAccount
            {
              GameAbbreviation = args.Game.ToLower(),
              ServerCode = args.Server.ToUpper(),
              NormalizedSummonerName = _utilities.NormalizeSummonerName(args.SummonerName),
              BroadcasterId = broadcaster.Id
            });

          if (account != null)
          {
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_add_accountAlreadyExists"], message.DisplayName, account.ToStringShort()));
            _logger.LogInformation("Account \"{account}\" already exists in the database - returning", account.ToStringShort());

            return false;
          }

          // get data about the account from Riot Summoner API
          if (args.Game.ToLower() == "lol")
          {
            //data = await _riotLolApiHelper.SummonerGetByName(args.SummonerName, serverApiCode);
            data = await _summonerV4.GetSummonerByName(args.SummonerName, Enum.Parse<Server>(args.Server));
          }
          else if (args.Game.ToLower() == "tft")
          {
            data = await _riotTftApiHelper.SummonerGetByName(args.SummonerName, serverApiCode);
          }
          else
          {
            data = null;
          }

          if (data == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
            _logger.LogInformation("Loading account data failed - returning");

            return false;
          }

          account = new RiotAccount
          {
            BroadcasterId = broadcaster.Id,
            GameAbbreviation = args.Game.ToLower(),
            SummonerName = data.Name,
            NormalizedSummonerName = _utilities.NormalizeSummonerName(data.Name),
            ServerCode = args.Server.ToUpper(),
            SummonerId = data.SummonerId,
            AccountId = data.AccountId,
            Puuid = data.Puuid
          };
          await _riotAccounts.InsertAsync(account);
          account = await _riotAccounts.FindAsync(
            "SummonerId = @SummonerId AND GameAbbreviation = @GameAbbreviation AND BroadcasterId = @BroadcasterId",
            new RiotAccount {SummonerId = account.SummonerId, GameAbbreviation = account.GameAbbreviation, BroadcasterId = broadcaster.Id});
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_add_accountAdded"], message.DisplayName, account.ToStringList()));

          break;
        }
        case "remove": // \account remove <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccounts.FindAsync("Id = @Id", new RiotAccount {Id = args.AccountId});

          if (account == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          // delete the account
          await _riotAccounts.DeleteAsync("Id = @Id", new RiotAccount {Id = args.AccountId});

          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_remove_accountRemoved"], message.DisplayName, account.ToStringShort()));

          break;
        }
        case "toggleactive": // \account toggleactive <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccounts.FindAsync("Id = @Id", new RiotAccount {Id = args.AccountId});

          if (account == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          account.Active = !account.Active;
          await _riotAccounts.UpdateAsync(account);
          _client.SendMessage(message.Channel,
                              string.Format(Globals.Locale["account_toggleactive_toggled"], message.DisplayName, account.ToStringShort(),
                                            account.Active ? Globals.Locale["account_value_active"] : Globals.Locale["account_value_inactive"]));

          break;
        }
        case "display": // \account display <AccountId> <NewDisplayName>
        {
          // check if account with given id exists
          account = await _riotAccounts.FindAsync("Id = @Id", new RiotAccount {Id = args.AccountId});

          if (account == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          account.DisplayName = args.NewDisplayName == "-" ? "" : args.NewDisplayName;
          await _riotAccounts.UpdateAsync(account);

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

          break;
        }
        case "update": // \account update <AccountId>
        {
          // check if account with given id exists
          account = await _riotAccounts.FindAsync("Id = @Id", new RiotAccount {Id = args.AccountId});

          if (account == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountDoesNotExist"], message.DisplayName, args.AccountId));
            _logger.LogInformation("Account with ID {id} doesn't exist in the database - returning", args.AccountId);

            return false;
          }

          if (account.GameAbbreviation.ToLower() == "lol")
          {
            data = await _summonerV4.GetSummonerByPuuid(account.Puuid, Enum.Parse<Server>(account.ServerCode));
          }
          else if (account.GameAbbreviation.ToLower() == "tft")
          {
            data = await _riotTftApiHelper.SummonerGetByAccountId(account.AccountId, _utilities.GetServerApiCode(account.ServerCode));
          }
          else
          {
            data = null;
          }

          if (data == null)
          {
            _client.SendMessage(message.Channel, string.Format(Globals.Locale["account_accountLoadingFailed"], message.DisplayName));
            _logger.LogInformation("Loading account data failed - returning");

            return false;
          }

          if (data.Name != account.SummonerName)
          {
            var oldSummonerName = account.SummonerName;
            account.SummonerName = data.Name;
            account.NormalizedSummonerName = _utilities.NormalizeSummonerName(data.Name);
            await _riotAccounts.UpdateAsync(account);
            _client.SendMessage(message.Channel,
                                string.Format(Globals.Locale["account_update_updated"], message.DisplayName, oldSummonerName, account.SummonerName));
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
