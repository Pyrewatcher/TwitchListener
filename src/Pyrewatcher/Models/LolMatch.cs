namespace Pyrewatcher.Models
{
  public class LolMatch
  {
    public long Timestamp { get; set; }
    public string Result { get; set; }
    public long ChampionId { get; set; }
    public string Kda { get; set; }
    public int ControlWardsBought { get; set; }
  }
}
