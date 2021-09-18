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

    public override string ToString()
    {
      if (Tier == null || Rank == null || LeaguePoints == null)
      {
        return Globals.Locale["ranga_value_unavailable"];
      }

      var output = Tier is "MASTER" or "GRANDMASTER" or "CHALLENGER" ? $"{Tier} {LeaguePoints} LP" : $"{Tier} {Rank} {LeaguePoints} LP";

      if (SeriesProgress != null)
      {
        output += $" ({SeriesProgress.Replace('N', '-').Replace('W', '✔').Replace('L', '✖')})";
      }

      return output;
    }
  }
}
