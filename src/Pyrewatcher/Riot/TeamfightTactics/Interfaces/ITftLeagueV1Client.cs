using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.TeamfightTactics.Models;

namespace Pyrewatcher.Riot.TeamfightTactics.Interfaces
{
  public interface ITftLeagueV1Client
  {
    Task<IEnumerable<TftLeagueEntryV1Dto>> GetLeagueEntriesBySummonerId(string summonerId, Server server);
  }
}