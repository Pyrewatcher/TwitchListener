using Pyrewatcher.Riot.Enums;

namespace Pyrewatcher.Models
{
  public class NewRiotAccount
  {
    public string Key { get; set; }
    public long ChannelId { get; set; }
    public string SummonerName { get; set; }
    public string NormalizedSummonerName { get; set; }
    public string ServerStr { get; set; }
    public Server Server { get; set; }
    public string GameStr { get; set; }
    public Game Game { get; set; }
    public string DisplayName { get; set; }
    public bool Active { get; set; }
    public string SummonerId { get; set; }
    public string AccountId { get; set; }
    public string Puuid { get; set; }
    public string Tier { get; set; }
    public string Rank { get; set; }
    public string LeaguePoints { get; set; }
    public string SeriesProgress { get; set; }

    public string DisplayableRank
    {
      get
      {
        if (Tier is null)
        {
          return null;
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
}
