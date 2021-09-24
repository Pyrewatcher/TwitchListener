using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface ILeagueV4Client
  {
    Task<IEnumerable<LeagueEntryV4Dto>> GetLeagueEntriesBySummonerId(string summonerId, Server server);
  }
}