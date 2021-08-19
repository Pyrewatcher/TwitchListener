namespace Pyrewatcher.Models
{
  public class LolParticipantDto
  {
    public int ParticipantId { get; set; }
    public long ChampionId { get; set; }
    public ParticipantStatsDto Stats { get; set; }
  }
}
