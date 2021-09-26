using System;

namespace Pyrewatcher.Riot.Utilities
{
  public static class RiotUtilities
  {
    private static DateTimeOffset GetStartTimeOffset()
    {
      if (DateTime.UtcNow - DateTime.Today < TimeSpan.FromHours(4))
      {
        var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        return new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 4, 00, 00, TimeSpan.Zero);
      }
      else
      {
        var today = DateTime.Today;

        return new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, TimeSpan.Zero);
      }
    }

    public static DateTime GetStartTime()
    {
      if (DateTime.UtcNow - DateTime.Today < TimeSpan.FromHours(4))
      {
        var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        return new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 4, 00, 00, DateTimeKind.Utc);
      }
      else
      {
        var today = DateTime.Today;

        return new DateTime(today.Year, today.Month, today.Day, 4, 00, 00, DateTimeKind.Utc);
      }
    }

    public static long GetStartTimeInSeconds()
    {
      return GetStartTimeOffset().ToUnixTimeSeconds();
    }

    public static long GetStartTimeInMilliseconds()
    {
      return GetStartTimeOffset().ToUnixTimeMilliseconds();
    }

    public static string NormalizeSummonerName(string summonerName)
    {
      var normalizedSummonerName = summonerName.Replace(" ", "").ToLower();

      return normalizedSummonerName;
    }
  }
}
