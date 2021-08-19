using System.Collections.Generic;

namespace Pyrewatcher.Models
{
  public class MatchlistDto
  {
    public List<MatchReferenceDto> Matches { get; set; }
    public string SummonerId { get; set; }
  }
}
