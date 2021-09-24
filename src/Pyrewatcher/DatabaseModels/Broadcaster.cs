using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Broadcaster
  {
    public long Id { get; set; }
    public bool Connected { get; set; } = false;
    public bool SubGreetingsEnabled { get; set; } = false;
    public string SubGreetingEmote { get; set; } = "HeyGuys";

    [Description("ignore")] public string DisplayName { get; set; }

    [Description("ignore")]
    public string Name
    {
      get => DisplayName.ToLower();
    }
  }
}
