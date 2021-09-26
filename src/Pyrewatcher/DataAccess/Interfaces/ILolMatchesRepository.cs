using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILolMatchesRepository
  {
    Task<IEnumerable<string>> NewGetMatchesNotInDatabase(List<string> matches);
    Task<IEnumerable<string>> NewGetMatchesToUpdateByKey(string accountKey, List<string> matches);
    Task<IEnumerable<NewLolMatch>> NewGetTodaysMatchesByChannelId(long channelId);
    Task<bool> NewInsertMatchFromDto(string matchId, MatchV5Dto match);
    Task<bool> NewInsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player);
  }
}