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
    Task<IEnumerable<RiotAccount>> GetActiveAccountsWithRankByBroadcasterIdAsync(long broadcasterId);
    Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByBroadcasterIdAsync(long broadcasterId);
    Task<IEnumerable<RiotAccount>> GetActiveTftAccountsForApiCallsByBroadcasterIdAsync(long broadcasterId);
    Task InsertAccount(RiotAccount account);
    Task<bool> ToggleActiveByIdAsync(long accountId);
    Task<bool> UpdateDisplayNameByIdAsync(long accountId, string displayName);
    Task<bool> UpdateRankByIdAsync(long accountId, string tier, string rank, string leaguePoints, string seriesProgress);
    Task<bool> UpdateSummonerNameByIdAsync(long accountId, string summonerName);
    Task<RiotAccount> GetAccountForApiCallsByIdAsync(long accountId);
  }
}