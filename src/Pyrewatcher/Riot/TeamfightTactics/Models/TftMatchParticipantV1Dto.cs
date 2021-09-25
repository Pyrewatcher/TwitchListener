using Newtonsoft.Json;

namespace Pyrewatcher.Riot.TeamfightTactics.Models
{
  public class TftMatchParticipantV1Dto
  {
    [JsonProperty("puuid")]
    public string Puuid { get; set; }
    [JsonProperty("placement")]
    public int Place { get; set; }
  }
}
