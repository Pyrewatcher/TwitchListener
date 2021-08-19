using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class CommandRepository : Repository<Command>
  {
    public override string TableName
    {
      get => "Commands";
    }

    public CommandRepository(IConfiguration config, ILogger<Repository<Command>> logger) : base(config, logger) { }
  }
}
