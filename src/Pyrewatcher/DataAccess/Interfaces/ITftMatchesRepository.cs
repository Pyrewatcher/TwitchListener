using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.TeamfightTactics.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ITftMatchesRepository
  {
    Task<IEnumerable<string>> GetMatchesNotInDatabaseAsync(List<string> matches);
    Task<IEnumerable<string>> GetMatchesToUpdateByKeyAsync(string accountKey, List<string> matches);
    Task<IEnumerable<TftMatch>> GetTodaysMatchesByChannelIdAsync(long channelId);
    Task<bool> InsertMatchFromDtoAsync(string matchId, TftMatchV1Dto match);
    Task<bool> InsertMatchPlayerFromDtoAsync(string accountKey, string matchId, TftMatchParticipantV1Dto player);
  }
}