using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class LocaleLineRepository : Repository<LocaleLine>
  {
    public override string TableName
    {
      get => "LocaleLines";
    }

    public LocaleLineRepository(IConfiguration config, ILogger<Repository<LocaleLine>> logger) : base(config, logger) { }
  }
}
