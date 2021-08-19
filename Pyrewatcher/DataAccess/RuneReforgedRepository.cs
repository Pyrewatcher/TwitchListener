using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class RuneReforgedRepository : Repository<RuneReforged>
  {
    public override string TableName
    {
      get => "RunesReforged";
    }

    public RuneReforgedRepository(IConfiguration config, ILogger<Repository<RuneReforged>> logger) : base(config, logger) { }
  }
}
