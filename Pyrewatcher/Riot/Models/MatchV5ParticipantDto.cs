﻿using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class MatchV5ParticipantDto
  {
    [JsonProperty("championId")]
    public int ChampionId { get; set; }
    [JsonProperty("kills")]
    public int Kills { get; set; }
    [JsonProperty("deaths")]
    public int Deaths { get; set; }
    [JsonProperty("assists")]
    public int Assists { get; set; }
    [JsonProperty("win")]
    public bool WonMatch { get; set; }
    [JsonProperty("visionWardsBoughtInGame")]
    public int VisionWardsBought { get; set; }
    [JsonProperty("puuid")]
    public string Puuid { get; set; }
    [JsonProperty("summonerName")]
    public string SummonerName { get; set; }
    [JsonProperty("summonerId")]
    public string SummonerId { get; set; }
  }
}
