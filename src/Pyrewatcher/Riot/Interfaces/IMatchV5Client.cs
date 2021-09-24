using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface IMatchV5Client
  {
    Task<IEnumerable<string>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, long? startTime = null, long? endTime = null,
                                                           int? queue = null, string type = null, int? start = null, int? count = null);
    Task<MatchV5Dto> GetMatchById(string matchId, RoutingValue routingValue);
  }
}
