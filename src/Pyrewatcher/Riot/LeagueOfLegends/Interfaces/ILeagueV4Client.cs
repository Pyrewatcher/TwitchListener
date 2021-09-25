using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Riot.LeagueOfLegends.Interfaces
{
  public interface ILeagueV4Client
  {
    Task<IEnumerable<LeagueEntryV4Dto>> GetLeagueEntriesBySummonerId(string summonerId, Server server);
  }
}