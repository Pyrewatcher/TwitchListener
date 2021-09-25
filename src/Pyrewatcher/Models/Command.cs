namespace Pyrewatcher.Models
{
  public class Command
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public string Channel { get; set; }
    public string Type { get; set; }
    public bool IsAdministrative { get; set; }
    public bool IsPublic { get; set; }
    public int Cooldown { get; set; }
    public long UsageCount { get; set; }
  }
}
