namespace Pyrewatcher.Riot.Interfaces
{
  public interface IRiotClient
  {
    public ILeagueV4Client LeagueV4 { get; }
    public IMatchV5Client MatchV5 { get; }
    public ISummonerV4Client SummonerV4 { get; }
  }
}
