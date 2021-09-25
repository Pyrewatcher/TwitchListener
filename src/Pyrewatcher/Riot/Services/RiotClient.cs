using Pyrewatcher.Riot.Interfaces;

namespace Pyrewatcher.Riot.Services
{
  public class RiotClient : IRiotClient
  {
    public ILeagueV4Client LeagueV4 { get; }
    public IMatchV5Client MatchV5 { get; }
    public ISpectatorV4Client SpectatorV4 { get; }
    public ISummonerV4Client SummonerV4 { get; }
    public ITftSummonerV1Client TftSummonerV1 { get; }

    public RiotClient(ILeagueV4Client leagueV4, IMatchV5Client matchV5, ISpectatorV4Client spectatorV4, ISummonerV4Client summonerV4,
                      ITftSummonerV1Client tftSummonerV1)
    {
      LeagueV4 = leagueV4;
      MatchV5 = matchV5;
      SpectatorV4 = spectatorV4;
      SummonerV4 = summonerV4;
      TftSummonerV1 = tftSummonerV1;
    }
  }
}
