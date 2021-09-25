using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class TftSummonerV1Dto
  {
    [JsonProperty("accountId")]
    public string AccountId { get; set; }
    [JsonProperty("id")]
    public string SummonerId { get; set; }
    [JsonProperty("puuid")]
    public string Puuid { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
  }
}
