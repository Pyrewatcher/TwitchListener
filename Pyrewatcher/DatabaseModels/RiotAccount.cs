using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class RiotAccount
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long BroadcasterId { get; set; }
    public string GameAbbreviation { get; set; }
    public string SummonerName { get; set; }
    public string NormalizedSummonerName { get; set; }
    public string ServerCode { get; set; }
    public string DisplayName { get; set; } = "";
    public bool Active { get; set; } = true;
    public string SummonerId { get; set; } = "";
    public string AccountId { get; set; } = "";
    public string Puuid { get; set; } = "";
    public string Tier { get; set; }
    public string Rank { get; set; }
    public string LeaguePoints { get; set; }
    public string SeriesProgress { get; set; }

    public string ToStringList()
    {
      if (string.IsNullOrEmpty(DisplayName))
      {
        return
          $"[{Id} {(Active ? Globals.Locale["account_value_active"] : Globals.Locale["account_value_inactive"])}] {GameAbbreviation.ToUpper()} {ServerCode} {SummonerName}";
      }
      else
      {
        return
          $"[{Id} {(Active ? Globals.Locale["account_value_active"] : Globals.Locale["account_value_inactive"])}] {GameAbbreviation.ToUpper()} {ServerCode} {SummonerName} ({DisplayName})";
      }
    }

    public string ToStringListactive()
    {
      if (string.IsNullOrEmpty(DisplayName))
      {
        return $"[{Id}] {GameAbbreviation.ToUpper()} {ServerCode} {SummonerName}";
      }
      else
      {
        return $"[{Id}] {GameAbbreviation.ToUpper()} {ServerCode} {SummonerName} ({DisplayName})";
      }
    }

    public string ToStringShort()
    {
      return $"{GameAbbreviation.ToUpper()} {ServerCode} {SummonerName}";
    }
  }
}
