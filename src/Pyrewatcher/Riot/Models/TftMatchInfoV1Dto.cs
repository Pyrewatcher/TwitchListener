using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class TftMatchInfoV1Dto
  {
    [JsonProperty("game_datetime")]
    public long Timestamp { get; set; }
    [JsonProperty("participants")]
    public IEnumerable<TftMatchParticipantV1Dto> Players { get; set; }
  }
}
