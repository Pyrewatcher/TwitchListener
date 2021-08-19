using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class BanRepository : Repository<Ban>
  {
    public override string TableName
    {
      get => "Bans";
    }

    public BanRepository(IConfiguration config, ILogger<Repository<Ban>> logger) : base(config, logger) { }
  }
}
