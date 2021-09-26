using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.TeamfightTactics.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ITftMatchesRepository
  {
    Task<IEnumerable<string>> NewGetMatchesNotInDatabase(List<string> matches);
    Task<IEnumerable<string>> NewGetMatchesToUpdateByKey(string accountKey, List<string> matches);
    Task<IEnumerable<NewTftMatch>> NewGetTodaysMatchesByChannelId(long channelId);
    Task<bool> NewInsertMatchFromDto(string matchId, TftMatchV1Dto match);
    Task<bool> NewInsertMatchPlayerFromDto(string accountKey, string matchId, TftMatchParticipantV1Dto player);
  }
}