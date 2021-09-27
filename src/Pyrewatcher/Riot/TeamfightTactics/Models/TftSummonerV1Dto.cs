using Newtonsoft.Json;
using Pyrewatcher.Riot.Interfaces;

namespace Pyrewatcher.Riot.TeamfightTactics.Models
{
  public class TftSummonerV1Dto : ISummonerDto
  {
    [JsonProperty("accountId")]
    public string AccountId { get; set; }
    [JsonProperty("id")]
    public string SummonerId { get; set; }
    [JsonProperty("puuid")]
    public string Puuid { get; set; }
    [JsonProperty("name")]
    public string SummonerName { get; set; }
  }
}
