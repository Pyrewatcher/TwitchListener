using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class MatchV5Dto
  {
    [JsonProperty("info")]
    public MatchInfoV5Dto Info { get; set; }
  }
}
