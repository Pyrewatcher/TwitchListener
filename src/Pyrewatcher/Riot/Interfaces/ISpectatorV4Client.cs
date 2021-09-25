using System.Threading.Tasks;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface ISpectatorV4Client
  {
    Task<CurrentGameInfoV4Dto> GetActiveGameBySummonerId(string summonerId, Server server);
  }
}