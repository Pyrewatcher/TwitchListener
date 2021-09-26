namespace Pyrewatcher.Models
{
  public class NewLolMatch
  {
    public bool WonMatch { get; set; }
    public long ChampionId { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int ControlWardsBought { get; set; }
  }
}
