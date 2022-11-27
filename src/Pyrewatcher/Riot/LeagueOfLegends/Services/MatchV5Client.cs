using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.LeagueOfLegends.Interfaces;
using Pyrewatcher.Riot.LeagueOfLegends.Models;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;
using System;

namespace Pyrewatcher.Riot.LeagueOfLegends.Services
{
  public class MatchV5Client : IMatchV5Client
  {
    private readonly IConfiguration _configuration;

    private readonly RiotRateLimiter _rateLimiter;

    public MatchV5Client(IConfiguration configuration, RiotRateLimiter rateLimiter)
    {
      _configuration = configuration;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(RoutingValue routingValue)
    {
      return new FlurlRequest($"https://{routingValue.ToString().ToLowerInvariant()}.api.riotgames.com/")
            .AppendPathSegments("lol", "match", "v5")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _configuration.GetSection("ApiKeys")["RiotLol"]);
    }

    public async Task<IEnumerable<string>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, long? startTime = null,
                                                                        long? endTime = null, int? queue = null, string type = null,
                                                                        int? start = null, int? count = null)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, routingValue))
      {
        return Array.Empty<string>();
      }

      var request = BaseRequest(routingValue).AppendPathSegments("matches", "by-puuid", puuid, "ids");

      if (startTime.HasValue)
      {
        request = request.SetQueryParam("startTime", startTime);
      }
      if (endTime.HasValue)
      {
        request = request.SetQueryParam("endTime", endTime);
      }
      if (queue.HasValue)
      {
        request = request.SetQueryParam("queue", queue);
      }
      if (!string.IsNullOrWhiteSpace(type))
      {
        request = request.SetQueryParam("type", type);
      }
      if (start.HasValue)
      {
        request = request.SetQueryParam("start", start);
      }
      if (count.HasValue)
      {
        request = request.SetQueryParam("count", count);
      }

      var response = await request.GetAsync<IEnumerable<string>>();

      return response ?? Array.Empty<string>();
    }

    public async Task<MatchV5Dto> GetMatchById(string matchId, RoutingValue routingValue)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, routingValue))
      {
        return null;
      }

      var request = BaseRequest(routingValue).AppendPathSegments("matches", matchId);

      var response = await request.GetAsync<MatchV5Dto>();

      return response;
    }
  }
}
