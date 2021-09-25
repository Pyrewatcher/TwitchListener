namespace Pyrewatcher.Models
{
  public class Broadcaster
  {
    public long Id { get; set; }
    public bool Connected { get; set; }
    public bool SubGreetingsEnabled { get; set; }
    public string SubGreetingEmote { get; set; }
    public string DisplayName { get; set; }

    public string Name
    {
      get => DisplayName.ToLower();
    }
  }
}
