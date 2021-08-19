namespace Pyrewatcher.Models
{
  public class SummonerDto
  {
    public string AccountId { get; set; }
    public string Id { get; set; }
    public string Puuid { get; set; }
    public string Name { get; set; }

    public string SummonerId
    {
      get => Id;
    }
  }
}
