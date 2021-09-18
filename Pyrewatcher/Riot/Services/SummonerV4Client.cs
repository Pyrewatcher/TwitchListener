using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Common.Interfaces;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.Riot.Services
{
  public class SummonerV4Client : ISummonerV4Client
  {
    private readonly IConfiguration _config;

    public SummonerV4Client(IConfiguration config)
    {
      _config = config;
    }

    private IFlurlRequest BaseRequest(Server server)
    {
      return new FlurlRequest($"https://{server.ToApiCode()}.api.riotgames.com/")
            .AppendPathSegments("lol", "summoner", "v4")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _config.GetValue<string>("ApiKeys:RiotLol"));
    }

    public async Task<IResponse<SummonerV4Dto>> GetSummonerByName(string summonerName, Server server)
    {
      var request = BaseRequest(server).AppendPathSegments("summoners", "by-name", summonerName);

      var response = await request.GetAsync<SummonerV4Dto>();

      return response;
    }

    public async Task<IResponse<SummonerV4Dto>> GetSummonerByPuuid(string puuid, Server server)
    {
      var request = BaseRequest(server).AppendPathSegments("summoners", "by-puuid", puuid);

      var response = await request.GetAsync<SummonerV4Dto>();

      return response;
    }
  }
}
