using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILolMatchesRepository
  {
    Task<IEnumerable<string>> GetMatchesNotInDatabaseAsync(List<string> matches);
    Task<IEnumerable<string>> GetMatchesToUpdateByKeyAsync(string accountKey, List<string> matches);
    Task<IEnumerable<LolMatch>> GetTodaysMatchesByChannelIdAsync(long channelId);
    Task<bool> InsertMatchFromDtoAsync(string matchId, MatchV5Dto match);
    Task<bool> InsertMatchPlayerFromDtoAsync(string accountKey, string matchId, MatchParticipantV5Dto player);
  }
}