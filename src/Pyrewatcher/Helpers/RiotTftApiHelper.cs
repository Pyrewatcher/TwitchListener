using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Models;

namespace Pyrewatcher.Helpers
{
  public class RiotTftApiHelper
  {
    public HttpClient ApiClient { get; set; }
    private readonly IConfiguration _config;
    private readonly Utilities _utilities;

    public RiotTftApiHelper(Utilities utilities, IConfiguration config)
    {
      _utilities = utilities;
      _config = config;

      ApiClient = new HttpClient();
      ApiClient.DefaultRequestHeaders.Accept.Clear();
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      ApiClient.DefaultRequestHeaders.Add("X-Riot-Token", _config.GetSection("ApiKeys")["RiotTft"]);
    }

    public async Task<List<LeagueEntryDto>> LeagueGetByRiotAccountsList(List<RiotAccount> accountsList)
    {
      var tasks = new List<Task<HttpResponseMessage>>();

      foreach (var account in accountsList)
      {
        var serverApiCode = _utilities.GetServerApiCode(account.ServerCode);
        tasks.Add(ApiClient.GetAsync($"https://{serverApiCode}.api.riotgames.com/tft/league/v1/entries/by-summoner/{account.SummonerId}"));
      }

      var responses = await Task.WhenAll(tasks);

      var output = new List<LeagueEntryDto>();

      var i = 0;

      foreach (var response in responses)
      {
        //Console.WriteLine("Riot TFT API call");
        if (response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsAsync<List<LeagueEntryDto>>();

          if (responseContent == null)
          {
            output.Add(new LeagueEntryDto {SummonerId = accountsList[i].SummonerId});
          }
          else
          {
            var entry = responseContent.Find(x => x.QueueType == "RANKED_TFT");

            output.Add(entry ?? new LeagueEntryDto {SummonerId = accountsList[i].SummonerId});
          }
        }
        else
        {
          output.Add(new LeagueEntryDto {SummonerId = accountsList[i].SummonerId});
        }

        i++;
      }

      return output;
    }
  }
}
