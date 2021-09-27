using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IRiotAccountsRepository
  {
    Task<bool> NewDeleteChannelAccountByKey(string accountKey);
    Task<NewRiotAccount> NewGetChannelAccountForLookupByKeyAsync(long channelId, string accountKey);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveAccountsForDisplayByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetInactiveAccountsForDisplayByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveAccountsWithRankByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveLolAccountsForApiCallsByChannelIdAsync(long channelId);
    Task<IEnumerable<NewRiotAccount>> NewGetActiveTftAccountsForApiCallsByChannelIdAsync(long channelId);
    Task<bool> NewInsertAccountGameFromDto(Game game, Server server, ISummonerDto summoner);
    Task<bool> NewToggleActiveByKey(string accountKey);
    Task<bool> NewUpdateDisplayNameByKeyAsync(string accountKey, string displayName);
    Task<bool> NewUpdateRankByKeyAsync(string accountKey, string tier, string rank, string leaguePoints, string seriesProgress);
    Task<bool> NewUpdateSummonerNameByKeyAsync(string accountKey, string summonerName);
    Task<bool> NewIsAccountGameAssignedToChannel(long channelId, Game game, Server server, string summonerName);
    Task<bool> NewExistsAccountGame(Game game, Server server, string summonerName);
    Task<bool> NewAssignAccountGameToChannel(long channelId, Game game, Server server, string summonerName, string accountKey);
    Task<string> NewGetAccountSummonerName(Server server, string normalizedSummonerName);
  }
}