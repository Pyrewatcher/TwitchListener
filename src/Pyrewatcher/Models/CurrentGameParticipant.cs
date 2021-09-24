namespace Pyrewatcher.Models
{
  public class CurrentGameParticipant
  {
    public string SummonerId { get; set; }
    public Perks Perks { get; set; }
    public long TeamId { get; set; }
  }
}
