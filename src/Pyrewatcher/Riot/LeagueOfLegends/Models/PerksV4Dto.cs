using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pyrewatcher.Riot.LeagueOfLegends.Models
{
  public class PerksV4Dto
  {
    [JsonProperty("perkIds")]
    public IEnumerable<long> RuneIds { get; set; }
    [JsonProperty("perkStyle")]
    public long PrimaryPathId { get; set; }
    [JsonProperty("perkSubStyle")]
    public long SecondaryPathId { get; set; }
  }
}
