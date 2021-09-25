using Newtonsoft.Json;

namespace Pyrewatcher.Riot.TeamfightTactics.Models
{
  public class TftLeagueEntryV1Dto
  {
    [JsonProperty("queueType")]
    public string QueueType { get; set; }
    [JsonProperty("tier")]
    public string Tier { get; set; }
    [JsonProperty("rank")]
    public string Rank { get; set; }
    [JsonProperty("leaguePoints")]
    public string LeaguePoints { get; set; }
  }
}
