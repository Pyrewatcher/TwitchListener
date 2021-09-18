using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Common.Interfaces;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface IMatchV5Client : IRiotClient
  {
    Task<IResponse<IEnumerable<string>>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, long? startTime = null, long? endTime = null,
                                                           int? queue = null, string type = null, int? start = null, int? count = null);
    Task<IResponse<MatchV5Dto>> GetMatchById(string matchId, RoutingValue routingValue);
  }
}
