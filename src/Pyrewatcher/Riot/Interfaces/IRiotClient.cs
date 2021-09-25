using Pyrewatcher.Riot.LeagueOfLegends.Interfaces;
using Pyrewatcher.Riot.TeamfightTactics.Interfaces;

namespace Pyrewatcher.Riot.Interfaces
{
  public interface IRiotClient
  {
    public ILeagueV4Client LeagueV4 { get; }
    public IMatchV5Client MatchV5 { get; }
    public ISpectatorV4Client SpectatorV4 { get; }
    public ISummonerV4Client SummonerV4 { get; }
    public ITftLeagueV1Client TftLeagueV1 { get; }
    public ITftMatchV1Client TftMatchV1 { get; }
    public ITftSummonerV1Client TftSummonerV1 { get; }
  }
}
