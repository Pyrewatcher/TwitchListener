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
  }
}
