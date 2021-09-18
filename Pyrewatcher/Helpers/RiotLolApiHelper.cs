using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Models;

namespace Pyrewatcher.Helpers
{
  public class RiotLolApiHelper
  {
    public HttpClient ApiClient { get; set; }
    private readonly IConfiguration _config;
    private readonly Utilities _utilities;

    public RiotLolApiHelper(Utilities utilities, IConfiguration config)
    {
      _utilities = utilities;
      _config = config;

      ApiClient = new HttpClient();
      ApiClient.DefaultRequestHeaders.Accept.Clear();
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      ApiClient.DefaultRequestHeaders.Add("X-Riot-Token", _config.GetSection("ApiKeys")["RiotLol"]);
    }
    
    public async Task<(CurrentGameInfo gameInfo, RiotAccount activeAccount)> SpectatorGetOneByRiotAccountModelsList(List<RiotAccount> accountsList)
    {
      var tasks = new List<Task<HttpResponseMessage>>();

      foreach (var account in accountsList)
      {
        var serverApiCode = _utilities.GetServerApiCode(account.ServerCode);
        tasks.Add(ApiClient.GetAsync($"https://{serverApiCode}.api.riotgames.com/lol/spectator/v4/active-games/by-summoner/{account.SummonerId}"));
      }

      var responses = await Task.WhenAll(tasks);

      for (var i = 0; i < accountsList.Count; i++)
      {
        //Console.WriteLine("Riot LoL API call");
        if (responses[i].IsSuccessStatusCode)
        {
          var responseContent = await responses[i].Content.ReadAsAsync<CurrentGameInfo>();

          if (responseContent != null)
          {
            return (responseContent, accountsList[i]);
          }
        }
      }

      return (null, null);
    }
  }
}
