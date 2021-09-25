using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DatabaseModels;
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

    public async Task<List<List<string>>> MatchGetMatchlistsByRiotAccountsList(List<RiotAccount> accountsList)
    {
      var tasks = new List<Task<HttpResponseMessage>>();

      foreach (var account in accountsList)
      {
        var routingValue = _utilities.GetTftRoutingValue(account.ServerCode);
        tasks.Add(ApiClient.GetAsync($"https://{routingValue}.api.riotgames.com/tft/match/v1/matches/by-puuid/{account.Puuid}/ids?count=10"));
      }

      var responses = await Task.WhenAll(tasks);

      var output = new List<List<string>>();

      var i = 0;

      foreach (var response in responses)
      {
        //Console.WriteLine("Riot TFT API call");
        if (response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsAsync<List<string>>();

          output.Add(responseContent ?? new List<string>());
        }
        else
        {
          output.Add(new List<string>());
        }

        i++;
      }

      return output;
    }

    public async Task<List<TftMatchDto>> MatchGetByMatchesList(List<TftMatch> matchesList)
    {
      var toGet = new List<TftMatch>(matchesList);
      var output = new List<TftMatchDto>();

      while (toGet.Count > 0)
      {
        var tasks = new List<Task<HttpResponseMessage>>();

        var toRemove = new List<TftMatch>();

        foreach (var match in toGet)
        {
          if (tasks.Count >= 10)
          {
            break;
          }

          var routingValue = _utilities.GetTftRoutingValue(match.MatchId[..match.MatchId.IndexOf('_')]);
          tasks.Add(ApiClient.GetAsync($"https://{routingValue}.api.riotgames.com/tft/match/v1/matches/{match.MatchId}"));
          toRemove.Add(match);
        }

        var responses = await Task.WhenAll(tasks);

        for (var i = 0; i < responses.Length; i++)
        {
          //Console.WriteLine("Riot TFT API call");
          if (responses[i].IsSuccessStatusCode)
          {
            var responseContent = await responses[i].Content.ReadAsAsync<TftMatchDto>();

            output.Add(responseContent ?? new TftMatchDto {Metadata = new MetadataDto {Match_Id = toGet[i].MatchId}});
          }
          else
          {
            output.Add(new TftMatchDto {Metadata = new MetadataDto {Match_Id = toGet[i].MatchId}});
          }
        }

        foreach (var match in toRemove)
        {
          toGet.Remove(match);
        }

        toRemove.Clear();

        await Task.Delay(TimeSpan.FromSeconds(2));
      }

      return output;
    }
  }
}
