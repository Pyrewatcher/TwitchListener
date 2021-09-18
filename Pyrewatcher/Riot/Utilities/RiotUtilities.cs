using System;

namespace Pyrewatcher.Riot.Utilities
{
  public static class RiotUtilities
  {
    public static long GetStartTime()
    {
      if (DateTime.UtcNow - DateTime.Today < TimeSpan.FromHours(4))
      {
        var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        return new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 4, 00, 00, TimeSpan.Zero).ToUnixTimeSeconds();
      }
      else
      {
        var today = DateTime.Today;

        var output = new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, TimeSpan.Zero).ToUnixTimeSeconds();

        return output;
      }
    }
  }
}
