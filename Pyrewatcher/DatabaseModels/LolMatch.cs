using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class LolMatch
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long MatchId { get; set; }
    public string ServerApiCode { get; set; }
    public long AccountId { get; set; }
    public long Timestamp { get; set; }
    public string Result { get; set; } = "";
    public long ChampionId { get; set; } = 0;
    public string Kda { get; set; } = "";
    public long GameDuration { get; set; } = 0;
    public int ControlWardsBought { get; set; } = 0;
  }
}
