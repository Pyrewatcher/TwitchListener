using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Models;

namespace Pyrewatcher.Helpers
{
  public class CommandHelpers
  {
    private readonly BanRepository _bans;
    private readonly IConfiguration _config;
    private readonly LolMatchRepository _lolMatches;
    private readonly RiotAccountRepository _riotAccounts;
    private readonly RiotLolApiHelper _riotLolApiHelper;
    private readonly RiotTftApiHelper _riotTftApiHelper;
    private readonly TftMatchRepository _tftMatches;
    private readonly TwitchApiHelper _twitchApiHelper;
    private readonly UserRepository _users;

    public CommandHelpers(BanRepository bans, RiotAccountRepository riotAccounts, LolMatchRepository lolMatches, UserRepository users,
                          TftMatchRepository tftMatches, RiotLolApiHelper riotLolApiHelper, RiotTftApiHelper riotTftApiHelper, IConfiguration config,
                          TwitchApiHelper twitchApiHelper)
    {
      _bans = bans;
      _riotAccounts = riotAccounts;
      _lolMatches = lolMatches;
      _users = users;
      _tftMatches = tftMatches;
      _riotLolApiHelper = riotLolApiHelper;
      _riotTftApiHelper = riotTftApiHelper;
      _config = config;
      _twitchApiHelper = twitchApiHelper;
    }

    public async Task<bool> IsUserPermitted(User user, Command command)
    {
      if (await _bans.FindAsync("UserId = @UserId", new Ban {UserId = user.Id}) != null)
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
      if (response.Chatters == null)
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

      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcaster.Id, GameAbbreviation = "lol", Active = true})).ToList();

      // skip if no active accounts for update
      if (accountsList.Count == 0)
      {
        return;
      }

      var matchlists = await _riotLolApiHelper.MatchGetMatchlistsByRiotAccountsList(accountsList);

      var zippedList = accountsList.Zip(matchlists).ToList();

      foreach ((var account, var matchlist) in zippedList)
      {
        if (matchlist.Matches != null)
        {
          var matches = matchlist.Matches.Select(x => new LolMatch
                                  {
                                    AccountId = account.Id,
                                    MatchId = x.GameId,
                                    ServerApiCode = x.PlatformId.ToLower(),
                                    Timestamp = x.Timestamp
                                  })
                                 .ToList();
          await _lolMatches.InsertRangeIfNotExistsAsync(matches);
        }
      }

      var matchesToUpdate = (await _lolMatches.FindRangeAsync("Result = @Result", new LolMatch {Result = ""})).ToList();

      if (matchesToUpdate.Count > 0)
      {
        var matchDataList = await _riotLolApiHelper.MatchGetByMatchesList(matchesToUpdate);

        var zippedMatchList = matchesToUpdate.Zip(matchDataList).ToList();

        foreach ((var match, var matchData) in zippedMatchList)
        {
          var account = accountsList.Find(x => x.Id == matchesToUpdate.Find(y => y.MatchId == matchData.GameId).AccountId);

          if (account == null)
          {
            continue;
          }

          var participantIdentity =
            matchData.ParticipantIdentities.Find(x => x.Player.AccountId == account.AccountId || x.Player.CurrentAccountId == account.AccountId);

          if (participantIdentity == null)
          {
            continue;
          }

          var participant = matchData.Participants.Find(x => x.ParticipantId == participantIdentity.ParticipantId);

          if (participant == null)
          {
            continue;
          }

          match.ChampionId = participant.ChampionId;
          match.Result = participant.Stats.Win ? "W" : "L";
          match.Kda = $"{participant.Stats.Kills}/{participant.Stats.Deaths}/{participant.Stats.Assists}";
          match.GameDuration = matchData.GameDuration;
          match.ControlWardsBought = participant.Stats.VisionWardsBoughtInGame;

          await _lolMatches.UpdateAsync(match);
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

      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcaster.Id, GameAbbreviation = "tft", Active = true})).ToList();

      // skip if no active accounts for update
      if (accountsList.Count == 0)
      {
        return;
      }

      var matchlists = await _riotTftApiHelper.MatchGetMatchlistsByRiotAccountsList(accountsList);

      var zippedList = accountsList.Zip(matchlists).ToList();

      foreach ((var account, var matchlist) in zippedList)
      {
        if (matchlist.Count > 0)
        {
          var matches = matchlist.Select(x => new TftMatch {AccountId = account.Id, MatchId = x}).ToList();
          await _tftMatches.InsertRangeIfNotExistsAsync(matches);
        }
      }

      var matchesToUpdate = (await _tftMatches.FindRangeAsync("Place = @Place", new TftMatch {Place = 0})).ToList();

      if (matchesToUpdate.Count > 0)
      {
        var matchDataList = await _riotTftApiHelper.MatchGetByMatchesList(matchesToUpdate);

        var zippedMatchList = matchesToUpdate.Zip(matchDataList).ToList();

        foreach ((var match, var matchData) in zippedMatchList)
        {
          if (matchData.Info == null)
          {
            continue;
          }

          var account = accountsList.Find(x => x.Id == matchesToUpdate.Find(y => y.MatchId == matchData.Metadata.Match_Id).AccountId);

          if (account == null)
          {
            continue;
          }

          var participant = matchData.Info.Participants.Find(x => x.Puuid == account.Puuid);

          if (participant == null)
          {
            continue;
          }

          match.Timestamp = matchData.Info.Game_Datetime;
          match.Place = participant.Placement;

          await _tftMatches.UpdateAsync(match);
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

      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcaster.Id, GameAbbreviation = "lol", Active = true})).ToList();

      if (accountsList.Count == 0)
      {
        return;
      }

      var entriesList = await _riotLolApiHelper.LeagueGetByRiotAccountsList(accountsList);

      var zippedList = accountsList.Zip(entriesList).ToList();

      foreach ((var account, var entry) in zippedList)
      {
        if (entry.Tier == null || entry.Rank == null || entry.LeaguePoints == null)
        {
          continue;
        }

        account.Tier = entry.Tier;
        account.Rank = entry.Rank;
        account.LeaguePoints = entry.LeaguePoints;
        account.SeriesProgress = entry.MiniSeries?.Progress;
        await _riotAccounts.UpdateAsync(account);
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

      var accountsList =
        (await _riotAccounts.FindRangeAsync("BroadcasterId = @BroadcasterId AND GameAbbreviation = @GameAbbreviation AND Active = @Active",
                                            new RiotAccount {BroadcasterId = broadcaster.Id, GameAbbreviation = "tft", Active = true})).ToList();

      if (accountsList.Count == 0)
      {
        return;
      }

      var entriesList = await _riotTftApiHelper.LeagueGetByRiotAccountsList(accountsList);

      var zippedList = accountsList.Zip(entriesList).ToList();

      foreach ((var account, var entry) in zippedList)
      {
        if (entry.Tier == null || entry.Rank == null || entry.LeaguePoints == null)
        {
          continue;
        }

        account.Tier = entry.Tier;
        account.Rank = entry.Rank;
        account.LeaguePoints = entry.LeaguePoints;
        await _riotAccounts.UpdateAsync(account);
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

      var user = await _users.FindAsync("Name = @Name", new User {Name = userName});

      if (user == null) // user does not exist in the database - retrieve user id from Twitch API
      {
        user = await _twitchApiHelper.GetUserByName(userName);

        if (user.Id is 0 or -1)
        {
          return null;
        }

        if (await _users.FindAsync("Id = @Id", user) != null)
        {
          await _users.UpdateAsync(user);
        }
        else
        {
          await _users.InsertAsync(user);
        }
      }

      return user;
    }
  }
}
