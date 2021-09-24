using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class RiotApiExceptionDetailsStatus
  {
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("status_code")]
    public int StatusCode { get; set; }
  }
}
