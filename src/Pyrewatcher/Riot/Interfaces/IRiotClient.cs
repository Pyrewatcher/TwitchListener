namespace Pyrewatcher.Riot.Interfaces
{
  public interface IRiotClient
  {
    public ILeagueV4Client LeagueV4 { get; }
    public IMatchV5Client MatchV5 { get; }
    public ISpectatorV4Client SpectatorV4 { get; }
    public ISummonerV4Client SummonerV4 { get; }
    public ITftSummonerV1Client TftSummonerV1 { get; }
  }
}
