using Newtonsoft.Json;
using Pyrewatcher.Models;

namespace Pyrewatcher.Riot.Models
{
  public class LeagueEntryV4Dto
  {
    [JsonProperty("queueType")]
    public string QueueType { get; set; }
    [JsonProperty("tier")]
    public string Tier { get; set; }
    [JsonProperty("rank")]
    public string Rank { get; set; }
    [JsonProperty("leaguePoints")]
    public string LeaguePoints { get; set; }
    [JsonProperty("miniSeries")]
    public LeagueEntryMiniSeriesV4Dto Series { get; set; }

    public string SeriesProgress
    {
      get => Series?.Progress;
    }
  }
}
