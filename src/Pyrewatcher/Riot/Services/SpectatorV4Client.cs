using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.Services
{
  public class SpectatorV4Client : ISpectatorV4Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public SpectatorV4Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("lol", "spectator", "v4")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotLol"));
    }

    public async Task<CurrentGameInfoV4Dto> GetActiveGameBySummonerId(string summonerId, Server server)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("active-games", "by-summoner", summonerId);

      var response = await request.GetAsync<CurrentGameInfoV4Dto>();

      return response;
    }
  }
}
