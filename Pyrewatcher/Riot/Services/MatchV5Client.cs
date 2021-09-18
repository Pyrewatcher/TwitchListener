using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Common.Interfaces;
using Pyrewatcher.Riot.Enums;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Services
{
  public class MatchV5Client : IMatchV5Client
  {
    private readonly IConfiguration _configuration;
    public MatchV5Client(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    private IFlurlRequest BaseRequest(RoutingValue routingValue)
    {
      return new FlurlRequest($"https://{routingValue.ToString().ToLowerInvariant()}.api.riotgames.com/")
            .AppendPathSegments("lol", "match", "v5")
            .WithTimeout(15)
            .WithHeader("X-Riot-Token", _configuration.GetSection("ApiKeys")["RiotLol"]);
    }

    public async Task<IResponse<IEnumerable<string>>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, long? startTime = null,
                                                                        long? endTime = null, int? queue = null, string type = null,
                                                                        int? start = null, int? count = null)
    {
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

      try
      {
        var response = await request.SendAsync(HttpMethod.Get);
        var content = await response.GetJsonAsync<IEnumerable<string>>();

        var output = new RiotResponse<IEnumerable<string>>(response.StatusCode, content);

        return output;
      }
      catch (FlurlHttpException exception)
      {
        var response = await exception.GetResponseJsonAsync<RiotApiExceptionDetails>();

        var output = new RiotResponse<IEnumerable<string>>(response.Status.StatusCode, null, response.Status.Message);

        return output;
      }
    }

    public async Task<IResponse<MatchV5Dto>> GetMatchById(string matchId, RoutingValue routingValue)
    {
      var request = BaseRequest(routingValue).AppendPathSegments("matches", matchId);

      try
      {
        var response = await request.SendAsync(HttpMethod.Get);
        var content = await response.GetJsonAsync<MatchV5Dto>();

        var output = new RiotResponse<MatchV5Dto>(response.StatusCode, content);

        return output;
      }
      catch (FlurlHttpException exception)
      {
        var response = await exception.GetResponseJsonAsync<RiotApiExceptionDetails>();

        var output = new RiotResponse<MatchV5Dto>(response.Status.StatusCode, null, response.Status.Message);

        return output;
      }
    }
  }
}
