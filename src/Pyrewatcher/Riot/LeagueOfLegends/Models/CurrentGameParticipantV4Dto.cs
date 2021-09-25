using Newtonsoft.Json;

namespace Pyrewatcher.Riot.LeagueOfLegends.Models
{
  public class CurrentGameParticipantV4Dto
  {
    [JsonProperty("summonerId")]
    public string SummonerId { get; set; }
    [JsonProperty("perks")]
    public PerksV4Dto Runes { get; set; }
    [JsonProperty("teamId")]
    public long TeamId { get; set; }
  }
}
