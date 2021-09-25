using Newtonsoft.Json;

namespace Pyrewatcher.Riot.LeagueOfLegends.Models
{
  public class LeagueEntryMiniSeriesV4Dto
  {
    [JsonProperty("progress")]
    public string Progress { get; set; }
  }
}
