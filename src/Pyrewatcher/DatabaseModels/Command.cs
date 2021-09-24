using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Command
  {
    [Description("autoincrement")] public long Id { get; set; }
    public string Name { get; set; }
    public string Channel { get; set; } = "";
    public string Type { get; set; } = "Custom";
    public bool IsAdministrative { get; set; } = false;
    public bool IsPublic { get; set; } = false;
    public int Cooldown { get; set; } = 30;
    public long UsageCount { get; set; } = 0;
  }
}
