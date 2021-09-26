using System;
using System.Collections.Generic;
using Pyrewatcher.Riot.Enums;

namespace Pyrewatcher.Riot.Models
{
  public class RiotRateLimiter
  {
    private readonly IDictionary<string, RiotRateLimiterBucket> _buckets;

    public RiotRateLimiter()
    {
      _buckets = new Dictionary<string, RiotRateLimiterBucket>();
    }

    public bool PickToken(Game game, Server server)
    {
      return PickToken($"{game}-{server}");
    }

    public bool PickToken(Game game, RoutingValue routingValue)
    {
      return PickToken($"{game}-{routingValue}");
    }

    public bool PickToken(string key)
    {
      var bucketExists = _buckets.TryGetValue(key, out var bucket);

      if (!bucketExists)
      {
        bucket = new RiotRateLimiterBucket(20, 100);
        _buckets.Add(key, bucket);
      }

      return bucket.PickToken();
    }
  }

  internal class RiotRateLimiterBucket
  {
    private readonly object _bucket;

    private readonly int _limitPerSecond;
    private DateTime _perSecondStart;
    private int _perSecondAvailableTokens;

    private readonly int _limitPerTwoMinutes;
    private DateTime _perTwoMinutesStart;
    private int _perTwoMinutesAvailableTokens;

    public RiotRateLimiterBucket(int limitPerSecond, int limitPerTwoMinutes)
    {
      _bucket = new object();

      _perSecondStart = DateTime.UtcNow - TimeSpan.FromMinutes(10);
      _perTwoMinutesStart = DateTime.UtcNow - TimeSpan.FromMinutes(10);
      
      _limitPerSecond = limitPerSecond;
      _limitPerTwoMinutes = limitPerTwoMinutes;
    }

    public bool PickToken()
    {
      lock (_bucket)
      {
        var utcNow = DateTime.UtcNow;

        var available = true;

        if (utcNow - _perSecondStart > TimeSpan.FromSeconds(1))
        {
          _perSecondStart = utcNow;
          _perSecondAvailableTokens = _limitPerSecond;
        }
        else if (_perSecondAvailableTokens == 0)
        {
          available = false;
        }

        if (utcNow - _perTwoMinutesStart > TimeSpan.FromMinutes(2))
        {
          _perTwoMinutesStart = utcNow;
          _perTwoMinutesAvailableTokens = _limitPerTwoMinutes;
        }
        else if (_perTwoMinutesAvailableTokens == 0)
        {
          available = false;
        }

        if (available)
        {
          _perSecondAvailableTokens--;
          _perTwoMinutesAvailableTokens--;
        }
        
        return available;
      }
    }
  }
}
