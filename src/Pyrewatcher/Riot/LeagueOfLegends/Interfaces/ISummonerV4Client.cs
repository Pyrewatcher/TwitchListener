using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Riot.LeagueOfLegends.Interfaces
{
  public interface ISummonerV4Client
  {
    Task<SummonerV4Dto> GetSummonerByName(string summonerName, Server server);
    Task<SummonerV4Dto> GetSummonerByPuuid(string puuid, Server server);
  }
}
