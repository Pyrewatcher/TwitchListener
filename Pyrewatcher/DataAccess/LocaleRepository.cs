using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class LocaleRepository : Repository<Locale>
  {
    public override string TableName
    {
      get => "Locales";
    }

    public LocaleRepository(IConfiguration config, ILogger<Repository<Locale>> logger) : base(config, logger) { }
  }
}
