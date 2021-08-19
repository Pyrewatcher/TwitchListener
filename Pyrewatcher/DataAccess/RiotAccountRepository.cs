using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class RiotAccountRepository : Repository<RiotAccount>
  {
    public override string TableName
    {
      get => "RiotAccounts";
    }

    public RiotAccountRepository(IConfiguration config, ILogger<Repository<RiotAccount>> logger) : base(config, logger) { }
  }
}
