using Pyrewatcher.Riot.Utilities;

namespace Pyrewatcher.DatabaseModels
{
  public class RiotAccount
  {
    public long Id { get; set; }
    public long BroadcasterId { get; set; }
    public string GameAbbreviation { get; set; }
    public string SummonerName { get; set; }
    public string NormalizedSummonerName { get; set; }
    public string ServerCode { get; set; }
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
        if (Tier == null || Rank == null || LeaguePoints == null)
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

    public RiotAccount()
    {
      // needed by Dapper
    }

    public RiotAccount(long broadcasterId, string gameAbbreviation, string summonerName, string serverCode, string summonerId, string accountId, string puuid)
    {
      BroadcasterId = broadcasterId;
      GameAbbreviation = gameAbbreviation.ToLower();
      SummonerName = summonerName;
      NormalizedSummonerName = RiotUtilities.NormalizeSummonerName(summonerName);
      ServerCode = serverCode.ToUpper();
      DisplayName = "";
      Active = true;
      SummonerId = summonerId;
      AccountId = accountId;
      Puuid = puuid;
    }

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
