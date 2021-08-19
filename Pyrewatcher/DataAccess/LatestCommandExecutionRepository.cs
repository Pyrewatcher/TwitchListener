using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class LatestCommandExecutionRepository : Repository<LatestCommandExecution>
  {
    public override string TableName
    {
      get => "LatestCommandExecutions";
    }

    public LatestCommandExecutionRepository(IConfiguration config, ILogger<Repository<LatestCommandExecution>> logger) : base(config, logger) { }
  }
}
