using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.TeamfightTactics.Interfaces;
using Pyrewatcher.Riot.TeamfightTactics.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.TeamfightTactics.Services
{
  public class TftMatchV1Client : ITftMatchV1Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public TftMatchV1Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(RoutingValue routingValue)
    {
      return new FlurlRequest($"https://{routingValue.ToString().ToLowerInvariant()}.api.riotgames.com/")
            .AppendPathSegments("tft", "match", "v1")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotTft"));
    }

    public async Task<IEnumerable<string>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, int? count = null)
    {
      if (!_rateLimiter.PickToken(Game.TeamfightTactics, routingValue))
      {
        return null;
      }

      var request = BaseRequest(routingValue).AppendPathSegments("matches", "by-puuid", puuid, "ids");

      if (count.HasValue)
      {
        request = request.SetQueryParam("count", count);
      }

      var response = await request.GetAsync<IEnumerable<string>>();

      return response;
    }

    public async Task<TftMatchV1Dto> GetMatchById(string matchId, RoutingValue routingValue)
    {
      if (!_rateLimiter.PickToken(Game.TeamfightTactics, routingValue))
      {
        return null;
      }

      var request = BaseRequest(routingValue).AppendPathSegments("matches", matchId);

      var response = await request.GetAsync<TftMatchV1Dto>();

      return response;
    }
  }
}
