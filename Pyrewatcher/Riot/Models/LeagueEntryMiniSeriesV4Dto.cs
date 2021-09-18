using Newtonsoft.Json;

namespace Pyrewatcher.Models
{
  public class LeagueEntryMiniSeriesV4Dto
  {
    [JsonProperty("progress")]
    public string Progress { get; set; }
  }
}
