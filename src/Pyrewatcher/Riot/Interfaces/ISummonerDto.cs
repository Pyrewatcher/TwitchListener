namespace Pyrewatcher.Riot.Interfaces
{
  public interface ISummonerDto
  {
    public string AccountId { get; set; }
    public string SummonerId { get; set; }
    public string Puuid { get; set; }
    public string SummonerName { get; set; }
  }
}
