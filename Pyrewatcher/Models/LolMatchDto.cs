using System.Collections.Generic;

namespace Pyrewatcher.Models
{
  public class LolMatchDto
  {
    public long GameId { get; set; }
    public long GameDuration { get; set; }
    public List<LolParticipantDto> Participants { get; set; }
    public List<ParticipantIdentityDto> ParticipantIdentities { get; set; }
  }
}
