using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Models;

namespace Pyrewatcher.Helpers
{
  public class TwitchApiHelper
  {
    public HttpClient ApiClient { get; set; }

    private readonly IConfiguration _config;

    public TwitchApiHelper(IConfiguration config)
    {
      _config = config;

      ApiClient = new HttpClient();
      ApiClient.DefaultRequestHeaders.Accept.Clear();
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      ApiClient.DefaultRequestHeaders.Add("Authorization", _config.GetSection("Twitch")["ApiToken"]);
      ApiClient.DefaultRequestHeaders.Add("Client-ID", _config.GetSection("Twitch")["ClientId"]);
    }

    public async Task<ChattersResponse> GetChattersForBroadcaster(string broadcaster)
    {
      var response = await ApiClient.GetAsync($"https://tmi.twitch.tv/group/user/{broadcaster}/chatters");
      //Console.WriteLine("Twitch API call");

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsAsync<ChattersResponse>();

        if (responseContent == null)
        {
          return new ChattersResponse {BroadcasterName = broadcaster};
        }

        responseContent.BroadcasterName = broadcaster;

        return responseContent;
      }

      return new ChattersResponse {BroadcasterName = broadcaster};
    }

    public async Task<List<ChattersResponse>> GetChattersForBroadcasterList(List<Broadcaster> broadcasters)
    {
      var tasks = new List<Task<HttpResponseMessage>>();

      foreach (var broadcaster in broadcasters)
      {
        tasks.Add(ApiClient.GetAsync($"https://tmi.twitch.tv/group/user/{broadcaster.Name}/chatters"));
      }

      var responses = await Task.WhenAll(tasks);

      var output = new List<ChattersResponse>();

      for (var i = 0; i < broadcasters.Count; i++)
      {
        //Console.WriteLine("Twitch API call");
        if (responses[i].IsSuccessStatusCode)
        {
          var responseContent = await responses[i].Content.ReadAsAsync<ChattersResponse>();

          if (responseContent == null)
          {
            output.Add(new ChattersResponse {BroadcasterName = broadcasters[i].Name});
          }
          else
          {
            responseContent.BroadcasterName = broadcasters[i].Name;
            output.Add(responseContent);
          }
        }
        else
        {
          output.Add(new ChattersResponse {BroadcasterName = broadcasters[i].Name});
        }
      }

      return output;
    }

    public async Task<User> GetUserByName(string userName)
    {
      User output;

      var url = $"https://api.twitch.tv/helix/users?login={userName}";

      var response = await ApiClient.GetAsync(url);
      //Console.WriteLine("Twitch API call");

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsAsync<UserResponse>();

        if (responseContent.Data.Count > 0)
        {
          output = new User(responseContent.Data[0].Id, responseContent.Data[0].Display_Name);
        }
        else
        {
          output = new User(0, null);
        }
      }
      else
      {
        output = new User(-1, null);
      }

      return output;
    }
  }
}
