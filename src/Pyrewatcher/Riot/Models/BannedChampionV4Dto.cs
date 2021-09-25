using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class BannedChampionV4Dto
  {
    [JsonProperty("pickTurn")]
    public int OrderingKey { get; set; }
    [JsonProperty("championId")]
    public long ChampionId { get; set; }
    [JsonProperty("teamId")]
    public long TeamId { get; set; }
  }
}
