using System.Threading.Tasks;
using Pyrewatcher.Common.Interfaces;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface ISummonerV4Client : IRiotClient
  {
    Task<IResponse<SummonerV4Dto>> GetSummonerByName(string summonerName, Server server);
    Task<IResponse<SummonerV4Dto>> GetSummonerByPuuid(string puuid, Server server);
  }
}