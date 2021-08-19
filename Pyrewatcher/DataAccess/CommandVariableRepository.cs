using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class CommandVariableRepository : Repository<CommandVariable>
  {
    public override string TableName
    {
      get => "CommandVariables";
    }

    public CommandVariableRepository(IConfiguration config, ILogger<Repository<CommandVariable>> logger) : base(config, logger) { }
  }
}
