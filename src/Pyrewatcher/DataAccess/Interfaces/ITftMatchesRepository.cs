using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ITftMatchesRepository
  {
    Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches, long accountId);
    Task<IEnumerable<TftMatch>> GetTodaysMatchesByAccountId(long accountId);
    Task<bool> InsertFromDto(long accountId, string matchId, TftMatchV1Dto match, TftMatchParticipantV1Dto participant);
  }
}