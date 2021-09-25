using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.LeagueOfLegends.Interfaces;
using Pyrewatcher.Riot.LeagueOfLegends.Models;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.LeagueOfLegends.Services
{
  public class SummonerV4Client : ISummonerV4Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public SummonerV4Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("lol", "summoner", "v4")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotLol"));
    }

    public async Task<SummonerV4Dto> GetSummonerByName(string summonerName, Server server)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("summoners", "by-name", summonerName);

      var response = await request.GetAsync<SummonerV4Dto>();

      return response;
    }

    public async Task<SummonerV4Dto> GetSummonerByPuuid(string puuid, Server server)
    {
      if (!_rateLimiter.PickToken(Game.LeagueOfLegends, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("summoners", "by-puuid", puuid);

      var response = await request.GetAsync<SummonerV4Dto>();

      return response;
    }
  }
}
