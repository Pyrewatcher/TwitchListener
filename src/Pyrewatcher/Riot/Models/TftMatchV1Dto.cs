using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class TftMatchV1Dto
  {
    [JsonProperty("info")]
    public TftMatchInfoV1Dto Info { get; set; }
  }
}
