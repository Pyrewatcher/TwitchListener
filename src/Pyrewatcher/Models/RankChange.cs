using System;
using System.Linq;

namespace Pyrewatcher.Models
{
  public class RankChange
  {
    public string DisplayName { get; set; }
    public DateTime Timestamp { get; set; }
    public string OldTier { get; set; }
    public string OldRank { get; set; }
    public int OldLeaguePoints { get; set; }
    public string OldSeriesProgress { get; set; }
    public string NewTier { get; set; }
    public string NewRank { get; set; }
    public int NewLeaguePoints { get; set; }
    public string NewSeriesProgress { get; set; }

    public override string ToString()
    {
      // tier and rank did not change
      if (OldTier == NewTier && OldRank == NewRank)
      {
        // only league points changed
        if (OldLeaguePoints != NewLeaguePoints)
        {
          // series progress did not change - game won, lost or dodged outside of series
          if (NewSeriesProgress is null)
          {
            var won = NewLeaguePoints > OldLeaguePoints;
            var difference = Math.Abs(NewLeaguePoints - OldLeaguePoints);

            return $"{(won ? "✔ +" : "✖ -")}{difference}";
          }
          // series progress changed - game won, triggering series in the process
          else
          {
            var difference = NewLeaguePoints - OldLeaguePoints;

            return $"✔ {Globals.Locale["series"]} (+{difference})";
          }
        }
        else
        {
          // series progress changed - game won or lost in series
          if (OldSeriesProgress != NewSeriesProgress)
          {
            var wins = NewSeriesProgress.Count(x => x == 'W');
            var losses = NewSeriesProgress.Count(x => x == 'L');
            var won = NewSeriesProgress.TrimEnd('N').Last() == 'W';

            return $"{(won ? "✔" : "✖")} {wins}-{losses}";
          }
        }
      }
      // tier or rank changed - promoted or demoted
      else
      {
        var promoted = IsFirstRankHigher(NewTier, NewRank, OldTier, OldRank);

        return $"{(promoted ? "⮝" : "⮟")} {TierAbbreviation(NewTier)}{RankAbbreviation(NewRank)}";
      }

      return "??";
    }

    private bool IsFirstRankHigher(string firstTier, string firstRank, string secondTier, string secondRank)
    {
      return TierValue(firstTier) + RankValue(firstRank) > TierValue(secondTier) + RankValue(secondRank);
    }

    private static int TierValue(string tier)
    {
      return tier switch
      {
        "IRON" => 10,
        "BRONZE" => 20,
        "SILVER" => 30,
        "GOLD" => 40,
        "PLATINUM" => 50,
        "DIAMOND" => 60,
        "MASTER" => 70,
        "GRANDMASTER" => 80,
        "CHALLENGER" => 90,
        _ => 0
      };
    }

    private static int RankValue(string rank)
    {
      return rank switch
      {
        "I" => 4,
        "II" => 3,
        "III" => 2,
        "IV" => 1,
        _ => 0
      };
    }

    private static string TierAbbreviation(string tier)
    {
      return tier switch
      {
        "IRON" => "I",
        "BRONZE" => "B",
        "SILVER" => "S",
        "GOLD" => "G",
        "PLATINUM" => "P",
        "DIAMOND" => "D",
        "MASTER" => "MASTER",
        "GRANDMASTER" => "GRANDMASTER",
        "CHALLENGER" => "CHALLENGER",
        _ => "?"
      };
    }

    private static string RankAbbreviation(string rank)
    {
      return rank switch
      {
        "I" => "1",
        "II" => "2",
        "III" => "3",
        "IV" => "4",
        _ => "?"
      };
    }
  }
}
