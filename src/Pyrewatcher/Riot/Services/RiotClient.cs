using Pyrewatcher.Riot.Interfaces;

namespace Pyrewatcher.Riot.Services
{
  public class RiotClient : IRiotClient
  {
    public ILeagueV4Client LeagueV4 { get; }
    public IMatchV5Client MatchV5 { get; }
    public ISummonerV4Client SummonerV4 { get; }

    public RiotClient(ILeagueV4Client leagueV4, IMatchV5Client matchV5, ISummonerV4Client summonerV4)
    {
      LeagueV4 = leagueV4;
      MatchV5 = matchV5;
      SummonerV4 = summonerV4;
    }
  }
}
