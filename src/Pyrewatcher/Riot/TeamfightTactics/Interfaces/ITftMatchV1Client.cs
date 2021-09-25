using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.TeamfightTactics.Models;

namespace Pyrewatcher.Riot.TeamfightTactics.Interfaces
{
  public interface ITftMatchV1Client
  {
    Task<TftMatchV1Dto> GetMatchById(string matchId, RoutingValue routingValue);
    Task<IEnumerable<string>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, int? count = null);
  }
}