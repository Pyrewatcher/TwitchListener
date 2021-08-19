using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class LatestCommandExecution
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long BroadcasterId { get; set; }
    public long CommandId { get; set; }
    public string LatestExecution { get; set; } = "1970-01-01T00:00:00.0000000+00:00";
  }
}
