using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Alias
  {
    [Description("autoincrement")] public long Id { get; set; }
    public string Name { get; set; }
    public string NewName { get; set; }
    public long BroadcasterId { get; set; } = 0;
  }
}
