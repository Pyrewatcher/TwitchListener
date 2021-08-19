using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class TftMatch
  {
    [Description("autoincrement")] public long Id { get; set; }
    public string MatchId { get; set; }
    public long AccountId { get; set; }
    public long Timestamp { get; set; } = 0;
    public int Place { get; set; } = 0;
  }
}
