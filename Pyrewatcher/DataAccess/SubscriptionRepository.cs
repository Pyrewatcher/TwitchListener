using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class SubscriptionRepository : Repository<Subscription>
  {
    public override string TableName
    {
      get => "Subscriptions";
    }

    public SubscriptionRepository(IConfiguration config, ILogger<Repository<Subscription>> logger) : base(config, logger) { }
  }
}
