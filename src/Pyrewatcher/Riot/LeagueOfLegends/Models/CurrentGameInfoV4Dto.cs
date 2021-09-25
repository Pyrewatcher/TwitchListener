using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pyrewatcher.Riot.LeagueOfLegends.Models
{
  public class CurrentGameInfoV4Dto
  {
    [JsonProperty("participants")]
    public List<CurrentGameParticipantV4Dto> Players { get; set; }
    [JsonProperty("bannedChampions")]
    public List<BannedChampionV4Dto> BannedChampions { get; set; }
  }
}
