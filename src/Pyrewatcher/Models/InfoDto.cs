using System.Collections.Generic;

namespace Pyrewatcher.Models
{
  public class InfoDto
  {
    public long Game_Datetime { get; set; }
    public List<TftParticipantDto> Participants { get; set; }
  }
}
