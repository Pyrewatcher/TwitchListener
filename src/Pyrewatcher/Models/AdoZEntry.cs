namespace Pyrewatcher.Models
{
  public class AdoZEntry
  {
    public string ChampionName { get; set; }
    public bool GameWon { get; set; }
    public long Duration { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
  }
}
