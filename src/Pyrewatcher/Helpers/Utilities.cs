using System;

namespace Pyrewatcher.Helpers
{
  public class Utilities
  {
    public string GetServerApiCode(string serverCode)
    {
      return serverCode.ToUpper() switch
      {
        "EUNE" => "eun1",
        "EUW" => "euw1",
        _ => null
      };
    }

    public string GetTftRoutingValue(string serverCode)
    {
      return serverCode.ToUpper() switch
      {
        "EUNE" => "europe",
        "EUW" => "europe",
        "EUN1" => "europe",
        "EUW1" => "europe",
        _ => null
      };
    }

    public string GetGameFullName(string gameAbbreviation)
    {
      return gameAbbreviation.ToLower() switch
      {
        "lol" => "League of Legends",
        "tft" => "Teamfight Tactics",
        _ => null
      };
    }

    public long GetBeginTime()
    {
      if (DateTime.UtcNow - DateTime.Today < TimeSpan.FromHours(4))
      {
        var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        return new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 4, 00, 00, TimeSpan.Zero).ToUnixTimeMilliseconds();
      }
      else
      {
        var today = DateTime.Today;

        return new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, TimeSpan.Zero).ToUnixTimeMilliseconds();
      }
    }
  }
}
