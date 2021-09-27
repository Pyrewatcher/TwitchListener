using System;
using System.Security.Cryptography;
using System.Text;

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

    public static string NormalizeSummonerName(string summonerName)
    {
      var normalizedSummonerName = summonerName.Replace(" ", "").ToLower();

      return normalizedSummonerName;
    }

    // Source: https://stackoverflow.com/questions/36625891/create-a-unique-5-character-alphanumeric-string
    public static string GenerateAccountKey()
    {
      var builder = new StringBuilder();

      var possibleAlphaNumericValues = new[]
      {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1',
        '2', '3', '4', '5', '6', '7', '8', '9'
      };
      
      var upperBound = possibleAlphaNumericValues.Length - 1;
      var crypto = new RNGCryptoServiceProvider();

      for (var i = 1; i <= 5; i++)
      {
        var scale = uint.MaxValue;
        
        while (scale == uint.MaxValue)
        {
          var fourBytes = new byte[4];
          crypto.GetBytes(fourBytes);
          scale = BitConverter.ToUInt32(fourBytes, 0);
        }

        var scaledPercentageOfMax = scale / (double) uint.MaxValue;
        var scaledRange = upperBound * scaledPercentageOfMax;
        var index = (int) scaledRange;

        builder.Append(possibleAlphaNumericValues[index]);
      }

      return builder.ToString();
    }
  }
}
