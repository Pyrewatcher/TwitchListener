using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.Riot.Utilities
{
  public static class FlurlExtensions
  {
    public static async Task<RiotResponse<T>> GetAsync<T>(this IFlurlRequest request) where T : class
    {
      try
      {
        var response = await request.SendAsync(HttpMethod.Get);
        var content = await response.GetJsonAsync<T>();

        var output = new RiotResponse<T>(response.StatusCode, content);

        return output;
      }
      catch (FlurlHttpException exception)
      {
        var response = await exception.GetResponseJsonAsync<RiotApiExceptionDetails>();

        var output = new RiotResponse<T>(response.Status.StatusCode, default, response.Status.Message);

        return output;
      }
    }
  }
}
