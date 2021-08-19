using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Subscription
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long UserId { get; set; }
    public long BroadcasterId { get; set; }
    public string Type { get; set; }
    public string Plan { get; set; }
    public long EndingTimestamp { get; set; } = 0;
  }
}
