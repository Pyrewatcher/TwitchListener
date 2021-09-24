using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Utilities
{
  public static class FlurlExtensions
  {
    public static async Task<T> GetAsync<T>(this IFlurlRequest request) where T : class
    {
      try
      {
        var response = await request.SendAsync(HttpMethod.Get);
        var content = await response.GetJsonAsync<T>();
        
        return content;
      }
      catch (FlurlHttpException exception)
      {
        var response = await exception.GetResponseJsonAsync<RiotApiExceptionDetails>();

        return null;
      }
    }
  }
}
