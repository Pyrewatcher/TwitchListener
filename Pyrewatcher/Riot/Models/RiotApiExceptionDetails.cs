using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class RiotApiExceptionDetails
  {
    [JsonProperty("status")]
    public RiotApiExceptionDetailsStatus Status { get; set; }
  }
}
