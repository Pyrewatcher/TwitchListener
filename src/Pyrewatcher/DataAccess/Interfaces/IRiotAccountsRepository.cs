using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IRiotAccountsRepository
  {
    Task<bool> DeleteByIdAsync(long accountId);
    Task<RiotAccount> GetAccountForDisplayByDetailsAsync(string gameAbbreviation, string serverCode, string summonerName, long broadcasterId);
    Task<RiotAccount> GetAccountForDisplayByIdAsync(long accountId);
    Task<IEnumerable<RiotAccount>> GetAccountsByBroadcasterIdAsync(long broadcasterId);
    Task<IEnumerable<RiotAccount>> GetActiveAccountsByBroadcasterIdAsync(long broadcasterId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveAccountsWithRankByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveLolAccountsForApiCallsByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveTftAccountsForApiCallsByChannelIdAsync(long channelId);
    Task InsertAccount(RiotAccount account);
    Task<bool> ToggleActiveByIdAsync(long accountId);
    Task<bool> UpdateDisplayNameByIdAsync(long accountId, string displayName);
    Task<bool> NewUpdateRankByKeyAsync(string accountKey, string tier, string rank, string leaguePoints, string seriesProgress);
    Task<bool> UpdateSummonerNameByIdAsync(long accountId, string summonerName);
    Task<RiotAccount> GetAccountForApiCallsByIdAsync(long accountId);
  }
}