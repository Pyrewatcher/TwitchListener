using System.Collections.Generic;

namespace Pyrewatcher.Models
{
  public class CurrentGameInfo
  {
    public List<CurrentGameParticipant> Participants { get; set; }
    public List<BannedChampion> BannedChampions { get; set; }
  }
}
