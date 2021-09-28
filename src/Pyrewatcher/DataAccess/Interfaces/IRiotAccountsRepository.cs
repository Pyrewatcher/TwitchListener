using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IRiotAccountsRepository
  {
    Task<bool> UnassignAccountGameFromChannelByKeyAsync(string accountKey);
    Task<RiotAccount> GetChannelAccountForLookupByKeyAsync(long channelId, string accountKey);
    Task<IEnumerable<RiotAccount>> GetActiveAccountsForDisplayByChannelIdAsync(long channelId);
    Task<IEnumerable<RiotAccount>> GetInactiveAccountsForDisplayByChannelIdAsync(long channelId);
    Task<IEnumerable<RiotAccount>> GetActiveAccountsWithRankByChannelIdAsync(long channelId);
    Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByChannelIdAsync(long channelId);
    Task<IEnumerable<RiotAccount>> GetActiveTftAccountsForApiCallsByChannelIdAsync(long channelId);
    Task<bool> InsertAccountGameFromDtoAsync(Game game, Server server, ISummonerDto summoner);
    Task<bool> ToggleActiveByKeyAsync(string accountKey);
    Task<bool> UpdateDisplayNameByKeyAsync(string accountKey, string displayName);
    Task<bool> UpdateRankByKeyAsync(string accountKey, string tier, string rank, string leaguePoints, string seriesProgress);
    Task<bool> UpdateSummonerNameByKeyAsync(string accountKey, string summonerName);
    Task<bool> IsAccountGameAssignedToChannelAsync(long channelId, Game game, Server server, string summonerName);
    Task<bool> ExistsAccountGameAsync(Game game, Server server, string summonerName);
    Task<bool> AssignAccountGameToChannelAsync(long channelId, Game game, Server server, string summonerName, string accountKey);
    Task<string> GetAccountSummonerNameAsync(Server server, string normalizedSummonerName);
  }
}