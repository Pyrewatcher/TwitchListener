using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class MatchV5Dto
  {
    [JsonProperty("info")]
    public MatchV5InfoDto Info { get; set; }
  }
}
