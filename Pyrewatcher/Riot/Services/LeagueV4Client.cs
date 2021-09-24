using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.Services
{
  public class LeagueV4Client : ILeagueV4Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public LeagueV4Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("lol", "league", "v4")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotLol"));
    }

    public async Task<IEnumerable<LeagueEntryV4Dto>> GetLeagueEntriesBySummonerId(string summonerId, Server server)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("entries", "by-summoner", summonerId);

      var response = await request.GetAsync<IEnumerable<LeagueEntryV4Dto>>();

      return response;
    }
  }
}
