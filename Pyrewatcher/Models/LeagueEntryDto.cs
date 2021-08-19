namespace Pyrewatcher.Models
{
  public class LeagueEntryDto
  {
    public string SummonerId { get; set; }
    public string QueueType { get; set; }
    public string Tier { get; set; }
    public string Rank { get; set; }
    public string LeaguePoints { get; set; }
    public MiniSeriesDto MiniSeries { get; set; }

    public string SeriesProgress
    {
      get => MiniSeries?.Progress;
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
