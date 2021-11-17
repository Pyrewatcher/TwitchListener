using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Helpers
{
  public class CommandHelpers
  {
    private readonly IConfiguration _config;

    private readonly IBansRepository _bansRepository;
    private readonly ILolMatchesRepository _lolMatchesRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;
    private readonly ITftMatchesRepository _tftMatchesRepository;
    private readonly IUsersRepository _usersRepository;
    
    private readonly TwitchApiHelper _twitchApiHelper;
    private readonly IRiotClient _riotClient;

    public CommandHelpers(IConfiguration config, IBansRepository bansRepository, ILolMatchesRepository lolMatchesRepository,
                          IRiotAccountsRepository riotAccountsRepository, ITftMatchesRepository tftMatchesRepository,
                          IUsersRepository usersRepository, TwitchApiHelper twitchApiHelper, IRiotClient riotClient)
    {
      _bansRepository = bansRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _lolMatchesRepository = lolMatchesRepository;
      _usersRepository = usersRepository;
      _tftMatchesRepository = tftMatchesRepository;
      _config = config;
      _twitchApiHelper = twitchApiHelper;
      _riotClient = riotClient;
    }

    public async Task<bool> IsUserPermitted(User user, Command command)
    {
      if (await _bansRepository.IsUserBannedByIdAsync(user.Id))
      {
        return false;
      }

      return user.IsAdministrator || command.IsPublic;
    }

    public async Task UpdateChattersForBroadcasters(List<Broadcaster> broadcasters)
    {
      foreach (var broadcaster in broadcasters)
      {
        await UpdateChattersForBroadcaster(broadcaster);
      }
    }

    public async Task UpdateChattersForBroadcaster(Broadcaster broadcaster)
    {
      if (broadcaster.Name == _config.GetValue<string>("Irc:Username"))
      {
        return;
      }

      LoadChattersFromChattersResponse(await _twitchApiHelper.GetChattersForBroadcaster(broadcaster.Name));
    }

    public void LoadChattersFromChattersResponse(ChattersResponse response)
    {
      if (response.Chatters is null)
      {
        return;
      }

      var chatters = new List<string>();

      chatters.AddRange(response.Chatters.Broadcaster);
      chatters.AddRange(response.Chatters.Vips);
      chatters.AddRange(response.Chatters.Moderators);
      chatters.AddRange(response.Chatters.Global_Mods);
      chatters.AddRange(response.Chatters.Staff);
      chatters.AddRange(response.Chatters.Admins);
      chatters.AddRange(response.Chatters.Viewers);

      if (Globals.BroadcasterViewers.ContainsKey(response.BroadcasterName))
      {
        Globals.BroadcasterViewers[response.BroadcasterName] = chatters;
      }
      else
      {
        Globals.BroadcasterViewers.Add(response.BroadcasterName, chatters);
      }
    }

    public async Task UpdateLolMatchDataForBroadcaster(Broadcaster broadcaster)
    {
      // skip if Pyrewatcher
      if (broadcaster.Name == _config.GetSection("Twitch")["Username"].ToLower())
      {
        return;
      }

      var accounts = (await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByChannelIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }
      
      var matchesToInsert = new List<string>();
      var matchesToUpdate = new List<(string, RiotAccount)>();

      foreach (var account in accounts)
      {
        var matches = await _riotClient.MatchV5.GetMatchesByPuuid(account.Puuid, account.Server.ToRoutingValue(),
                                                                  RiotUtilities.GetStartTimeInSeconds());

        if (matches is null || !matches.Any())
        {
          continue;
        }

        var matchesList = matches.ToList();

        var matchesNotInDatabase = (await _lolMatchesRepository.GetMatchesNotInDatabaseAsync(matchesList)).ToList();
        var matchesNotUpdated = (await _lolMatchesRepository.GetMatchesToUpdateByKeyAsync(account.Key, matchesList.Except(matchesNotInDatabase).ToList())).ToList();
        matchesToInsert.AddRange(matchesNotInDatabase);
        matchesToUpdate.AddRange(matchesNotInDatabase.Select(x => (x, account)));
        matchesToUpdate.AddRange(matchesNotUpdated.Select(x => (x, account)));
      }

      foreach ((var matchId, var account) in matchesToUpdate)
      {
        var match = await _riotClient.MatchV5.GetMatchById(matchId, account.Server.ToRoutingValue());

        if (match is null)
        {
          continue;
        }

        if (match.Info.QueueId is 2000 or 2010 or 2020)
        {
          continue;
        }

        if (matchesToInsert.Contains(matchId))
        {
          var matchInserted = await _lolMatchesRepository.InsertMatchFromDtoAsync(matchId, match);

          if (!matchInserted)
          {
            // TODO: Log failure
            continue;
          }
        }

        var player = match.Info.Players.FirstOrDefault(x => x.Puuid == account.Puuid);

        if (player is null)
        {
          // TODO: Log failure
          continue;
        }

        var playerInserted = await _lolMatchesRepository.InsertMatchPlayerFromDtoAsync(account.Key, matchId, player);

        if (!playerInserted)
        {
          // TODO: Log failure
        }
      }
    }

    public async Task UpdateLolMatchDataForBroadcasters(List<Broadcaster> broadcasters)
    {
      foreach (var broadcaster in broadcasters)
      {
        await UpdateLolMatchDataForBroadcaster(broadcaster);
      }
    }

    public async Task UpdateTftMatchDataForBroadcaster(Broadcaster broadcaster)
    {
      // skip if Pyrewatcher
      if (broadcaster.Name == _config.GetSection("Twitch")["Username"].ToLower())
      {
        return;
      }

      var accounts = (await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByChannelIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      var matchesToInsert = new List<string>();
      var matchesToUpdate = new List<(string, RiotAccount)>();

      foreach (var account in accounts)
      {
        var matches = await _riotClient.TftMatchV1.GetMatchesByPuuid(account.Puuid, account.Server.ToRoutingValue(), 10);

        if (matches is null || !matches.Any())
        {
          continue;
        }

        var matchesList = matches.ToList();

        var matchesNotInDatabase = (await _tftMatchesRepository.GetMatchesNotInDatabaseAsync(matchesList)).ToList();
        var matchesNotUpdated = (await _tftMatchesRepository.GetMatchesToUpdateByKeyAsync(account.Key, matchesList.Except(matchesNotInDatabase).ToList())).ToList();
        matchesToInsert.AddRange(matchesNotInDatabase);
        matchesToUpdate.AddRange(matchesNotInDatabase.Select(x => (x, account)));
        matchesToUpdate.AddRange(matchesNotUpdated.Select(x => (x, account)));
      }

      foreach ((var matchId, var account) in matchesToUpdate)
      {
        var match = await _riotClient.TftMatchV1.GetMatchById(matchId, account.Server.ToRoutingValue());

        if (match is null)
        {
          continue;
        }

        if (matchesToInsert.Contains(matchId))
        {
          var matchInserted = await _tftMatchesRepository.InsertMatchFromDtoAsync(matchId, match);

          if (!matchInserted)
          {
            // TODO: Log failure
            continue;
          }
        }

        var player = match.Info.Players.FirstOrDefault(x => x.Puuid == account.Puuid);

        if (player is null)
        {
          // TODO: Log failure
          continue;
        }

        var inserted = await _tftMatchesRepository.InsertMatchPlayerFromDtoAsync(account.Key, matchId, player);

        if (!inserted)
        {
          // TODO: Log failure
        }
      }
    }

    public async Task UpdateTftMatchDataForBroadcasters(List<Broadcaster> broadcasters)
    {
      foreach (var broadcaster in broadcasters)
      {
        await UpdateTftMatchDataForBroadcaster(broadcaster);
      }
    }

    public async Task UpdateLolRankDataForBroadcaster(Broadcaster broadcaster)
    {
      // skip if Pyrewatcher
      if (broadcaster.Name == _config.GetSection("Twitch")["Username"].ToLower())
      {
        return;
      }

      var accounts = (await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByChannelIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      foreach (var account in accounts)
      {
        var leagueEntries = await _riotClient.LeagueV4.GetLeagueEntriesBySummonerId(account.SummonerId, account.Server);

        if (leagueEntries is null)
        {
          continue;
        }

        var entry = leagueEntries.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5");

        var updated = entry is null
          ? await _riotAccountsRepository.UpdateRankByKeyAsync(account.Key, null, null, null, null)
          : await _riotAccountsRepository.UpdateRankByKeyAsync(account.Key, entry.Tier, entry.Rank, entry.LeaguePoints, entry.SeriesProgress);

        if (!updated)
        {
          // TODO: Log failure
        }

        if (entry is not null && (entry.Tier != account.Tier || entry.Rank != account.Rank || entry.LeaguePoints != account.LeaguePoints ||
                                  entry.SeriesProgress != account.SeriesProgress))
        {
          var inserted = await _riotAccountsRepository.InsertRankChangeByKeyAsync(account.Key, account.Tier, account.Rank, account.LeaguePoints,
                                                                                      account.SeriesProgress, entry.Tier, entry.Rank,
                                                                                      entry.LeaguePoints, entry.SeriesProgress);

          if (!inserted)
          {
            // TODO: Log failure
          }
        }
      }
    }

    public async Task UpdateLolRankDataForBroadcasters(List<Broadcaster> broadcasters)
    {
      foreach (var broadcaster in broadcasters)
      {
        await UpdateLolRankDataForBroadcaster(broadcaster);
      }
    }

    public async Task UpdateTftRankDataForBroadcaster(Broadcaster broadcaster)
    {
      // skip if Pyrewatcher
      if (broadcaster.Name == _config.GetSection("Twitch")["Username"].ToLower())
      {
        return;
      }

      var accounts = (await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByChannelIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      foreach (var account in accounts)
      {
        var leagueEntries = await _riotClient.TftLeagueV1.GetLeagueEntriesBySummonerId(account.SummonerId, account.Server);

        if (leagueEntries is null)
        {
          continue;
        }
        
        var entry = leagueEntries.FirstOrDefault(x => x.QueueType == "RANKED_TFT");

        var updated = entry is null
          ? await _riotAccountsRepository.UpdateRankByKeyAsync(account.Key, null, null, null, null)
          : await _riotAccountsRepository.UpdateRankByKeyAsync(account.Key, entry.Tier, entry.Rank, entry.LeaguePoints, null);

        if (!updated)
        {
          // TODO: Log failure
        }

        if (entry is not null && (entry.Tier != account.Tier || entry.Rank != account.Rank || entry.LeaguePoints != account.LeaguePoints))
        {
          {
            var inserted = await _riotAccountsRepository.InsertRankChangeByKeyAsync(account.Key, account.Tier, account.Rank, account.LeaguePoints,
                                                                                        account.SeriesProgress, entry.Tier, entry.Rank,
                                                                                        entry.LeaguePoints, null);

            if (!inserted)
            {
              // TODO: Log failure
            }
          }
        }
      }
    }

    public async Task UpdateTftRankDataForBroadcasters(List<Broadcaster> broadcasters)
    {
      foreach (var broadcaster in broadcasters)
      {
        await UpdateTftRankDataForBroadcaster(broadcaster);
      }
    }

    public async Task<User> GetUser(string userName)
    {
      userName = userName.ToLower().TrimStart('@');
      
      var user = await _usersRepository.GetUserByName(userName);

      if (user is null) // user does not exist in the database - retrieve user id from Twitch API
      {
        user = await _twitchApiHelper.GetUserByName(userName);

        if (user.Id is 0 or -1)
        {
          return null;
        }
        
        //if (await _usersRepository.FindAsync("Id = @Id", user) is not null)
        if (await _usersRepository.ExistsById(user.Id))
        {
          var updated = await _usersRepository.UpdateNameById(user.Id, user.DisplayName);

          if (!updated)
          {
            // TODO: Log failure
          }
        }
        else
        {
          var inserted = await _usersRepository.InsertUser(user);

          if (!inserted)
          {
            // TODO: Log failure
          }
        }
      }

      return user;
    }
  }
}
