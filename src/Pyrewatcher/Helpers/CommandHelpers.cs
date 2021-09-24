using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;
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
    private readonly LolMatchRepository _lolMatchesRepository;
    private readonly IRiotAccountsRepository _riotAccountsRepository;
    private readonly TftMatchRepository _tftMatchesRepository;
    private readonly UserRepository _usersRepository;

    private readonly RiotTftApiHelper _riotTftApiHelper;
    private readonly TwitchApiHelper _twitchApiHelper;
    private readonly IRiotClient _riotClient;

    public CommandHelpers(IConfiguration config, IBansRepository bansRepository, LolMatchRepository lolMatchesRepository,
                          IRiotAccountsRepository riotAccountsRepository, TftMatchRepository tftMatchesRepository, UserRepository usersRepository,
                          RiotTftApiHelper riotTftApiHelper, TwitchApiHelper twitchApiHelper, IRiotClient riotClient)
    {
      _bansRepository = bansRepository;
      _riotAccountsRepository = riotAccountsRepository;
      _lolMatchesRepository = lolMatchesRepository;
      _usersRepository = usersRepository;
      _tftMatchesRepository = tftMatchesRepository;
      _riotTftApiHelper = riotTftApiHelper;
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
      
      var accounts = (await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }
      
      var matchesToRequest = new List<(RiotAccount, string)>();

      foreach (var account in accounts)
      {
        var matches = await _riotClient.MatchV5.GetMatchesByPuuid(account.Puuid, RoutingValue.Europe, RiotUtilities.GetStartTime()); // TODO: Set routing value depending on server

        if (matches is null)
        {
          continue;
        }

        foreach (var matchId in matches)
        {
          var matchNumber = long.Parse(matchId.Split('_')[1]);

          if (await _lolMatchesRepository.FindAsync("[MatchId] = @MatchId", new LolMatch { MatchId = matchNumber }) is null && matchesToRequest.All(x => x.Item2 != matchId))
          {
            matchesToRequest.Add((account, matchId));
          }
        }
      }

      foreach ((var account, var matchId) in matchesToRequest)
      {
        var match = await _riotClient.MatchV5.GetMatchById(matchId, RoutingValue.Europe); // TODO: Set routing value depending on server

        if (match is null)
        {
          continue;
        }
        
        var participant = match.Info.Players.First(x => x.Puuid == account.Puuid);

        var databaseMatch = new LolMatch
        {
          AccountId = account.Id,
          MatchId = match.Info.Id,
          ServerApiCode = matchId.Split('_')[0],
          Timestamp = match.Info.Timestamp,
          ChampionId = participant.ChampionId,
          Result = participant.WonMatch ? "W" : "L",
          Kda = $"{participant.Kills}/{participant.Deaths}/{participant.Assists}",
          GameDuration = match.Info.Duration / 1000,
          ControlWardsBought = participant.VisionWardsBought
        };

        await _lolMatchesRepository.InsertAsync(databaseMatch);
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

      var accounts = (await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      var matchlists = await _riotTftApiHelper.MatchGetMatchlistsByRiotAccountsList(accounts);

      var zippedList = accounts.Zip(matchlists).ToList();

      foreach ((var account, var matchlist) in zippedList)
      {
        if (matchlist.Any())
        {
          var matches = matchlist.Select(x => new TftMatch {AccountId = account.Id, MatchId = x}).ToList();
          await _tftMatchesRepository.InsertRangeIfNotExistsAsync(matches);
        }
      }

      var matchesToUpdate = (await _tftMatchesRepository.FindRangeAsync("Place = @Place", new TftMatch {Place = 0})).ToList();

      if (matchesToUpdate.Any())
      {
        var matchDataList = await _riotTftApiHelper.MatchGetByMatchesList(matchesToUpdate);

        var zippedMatchList = matchesToUpdate.Zip(matchDataList).ToList();

        foreach ((var match, var matchData) in zippedMatchList)
        {
          if (matchData.Info is null)
          {
            continue;
          }

          var account = accounts.Find(x => x.Id == matchesToUpdate.Find(y => y.MatchId == matchData.Metadata.Match_Id).AccountId);

          if (account is null)
          {
            continue;
          }

          var participant = matchData.Info.Participants.Find(x => x.Puuid == account.Puuid);

          if (participant is null)
          {
            continue;
          }

          match.Timestamp = matchData.Info.Game_Datetime;
          match.Place = participant.Placement;

          await _tftMatchesRepository.UpdateAsync(match);
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

      var accounts = (await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      foreach (var account in accounts)
      {
        var leagueEntries = await _riotClient.LeagueV4.GetLeagueEntriesBySummonerId(account.SummonerId, Enum.Parse<Server>(account.ServerCode, true));

        var entry = leagueEntries?.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5");

        if (entry is null)
        {
          continue;
        }
        
        var updated = await _riotAccountsRepository.UpdateRankByIdAsync(account.Id, entry.Tier, entry.Rank, entry.LeaguePoints, entry.SeriesProgress);

        if (!updated)
        {
          // TODO: Log failure
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

      var accounts = (await _riotAccountsRepository.GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(broadcaster.Id)).ToList();

      // skip if no active accounts for update
      if (!accounts.Any())
      {
        return;
      }

      var entriesList = await _riotTftApiHelper.LeagueGetByRiotAccountsList(accounts);

      var zippedList = accounts.Zip(entriesList).ToList();

      foreach ((var account, var entry) in zippedList)
      {
        if (entry.Tier is null || entry.Rank is null || entry.LeaguePoints is null)
        {
          continue;
        }
        
        var updated = await _riotAccountsRepository.UpdateRankByIdAsync(account.Id, entry.Tier, entry.Rank, entry.LeaguePoints, null);

        if (!updated)
        {
          // TODO: Log failure
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

      var user = await _usersRepository.FindAsync("Name = @Name", new User {Name = userName});

      if (user is null) // user does not exist in the database - retrieve user id from Twitch API
      {
        user = await _twitchApiHelper.GetUserByName(userName);

        if (user.Id is 0 or -1)
        {
          return null;
        }

        if (await _usersRepository.FindAsync("Id = @Id", user) is not null)
        {
          await _usersRepository.UpdateAsync(user);
        }
        else
        {
          await _usersRepository.InsertAsync(user);
        }
      }

      return user;
    }
  }
}
