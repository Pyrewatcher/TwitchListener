using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.Services
{
  public class TftSummonerV1Client : ITftSummonerV1Client
  {
    private readonly IConfiguration _config;

    private readonly RiotRateLimiter _rateLimiter;

    public TftSummonerV1Client(IConfiguration config, RiotRateLimiter rateLimiter)
    {
      _config = config;
      _rateLimiter = rateLimiter;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("tft", "summoner", "v1")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotTft"));
    }

    public async Task<TftSummonerV1Dto> GetSummonerByName(string summonerName, Server server)
    {
      if (!_rateLimiter.PickToken(Game.TeamfightTactics, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("summoners", "by-name", summonerName);

      var response = await request.GetAsync<TftSummonerV1Dto>();

      return response;
    }

    public async Task<TftSummonerV1Dto> GetSummonerByPuuid(string puuid, Server server)
    {
      if (!_rateLimiter.PickToken(Game.TeamfightTactics, server))
      {
        return null;
      }

      var request = BaseRequest(server).AppendPathSegments("summoners", "by-puuid", puuid);

      var response = await request.GetAsync<TftSummonerV1Dto>();

      return response;
    }
  }
}
