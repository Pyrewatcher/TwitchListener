using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.TeamfightTactics.Models;

namespace Pyrewatcher.Riot.TeamfightTactics.Interfaces
{
  public interface ITftSummonerV1Client
  {
    Task<TftSummonerV1Dto> GetSummonerByName(string summonerName, Server server);
    Task<TftSummonerV1Dto> GetSummonerByPuuid(string puuid, Server server);
  }
}