using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Riot.LeagueOfLegends.Interfaces
{
  public interface ISpectatorV4Client
  {
    Task<CurrentGameInfoV4Dto> GetActiveGameBySummonerId(string summonerId, Server server);
  }
}