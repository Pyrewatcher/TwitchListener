using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface ISummonerV4Client
  {
    Task<SummonerV4Dto> GetSummonerByName(string summonerName, Server server);
    Task<SummonerV4Dto> GetSummonerByPuuid(string puuid, Server server);
  }
}
