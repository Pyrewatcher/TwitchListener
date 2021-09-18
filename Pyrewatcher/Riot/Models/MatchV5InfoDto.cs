using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class MatchV5InfoDto
  {
    [JsonProperty("gameId")]
    public long Id { get; set; }
    [JsonProperty("gameDuration")]
    public long Duration { get; set; }
    [JsonProperty("gameStartTimestamp")]
    public long Timestamp { get; set; }
    [JsonProperty("participants")]
    public IEnumerable<MatchV5ParticipantDto> Players { get; set; }
  }
}
