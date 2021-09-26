using System;
using Pyrewatcher.Riot.Enums;

namespace Pyrewatcher.Riot.Utilities
{
  public static class RiotExtensions
  {
    public static string ToApiCode(this Server server)
    {
      return server switch
      {
        Server.EUNE => "eun1",
        Server.EUW => "euw1",
        _ => throw new ArgumentOutOfRangeException(nameof(server), server, "This server is unsupported")
      };
    }

    public static Game? ToGameEnum(this string gameString)
    {
      return gameString.ToLower() switch
      {
        "lol" => Game.LeagueOfLegends,
        "tft" => Game.TeamfightTactics,
        _ => null
      };
    }

    public static RoutingValue ToRoutingValue(this Server server)
    {
      return server switch
      {
        Server.EUNE => RoutingValue.Europe,
        Server.EUW => RoutingValue.Europe,
        _ => throw new ArgumentOutOfRangeException(nameof(server), server, "This server is unsupported")
      };
    }
  }
}
