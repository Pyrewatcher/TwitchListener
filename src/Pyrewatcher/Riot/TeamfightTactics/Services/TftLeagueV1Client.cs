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
  public class TftLeagueV1Client : ITftLeagueV1Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public TftLeagueV1Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("tft", "league", "v1")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotTft"));
    }

    public async Task<IEnumerable<TftLeagueEntryV1Dto>> GetLeagueEntriesBySummonerId(string summonerId, Server server)
    {
      if (!_rateLimiter.PickToken(Game.TeamfightTactics, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("entries", "by-summoner", summonerId);

      var response = await request.GetAsync<IEnumerable<TftLeagueEntryV1Dto>>();

      return response;
    }
  }
}
