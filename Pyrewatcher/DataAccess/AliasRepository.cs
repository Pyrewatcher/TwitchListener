using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class AliasRepository : Repository<Alias>
  {
    public override string TableName
    {
      get => "Aliases";
    }

    public AliasRepository(IConfiguration config, ILogger<Repository<Alias>> logger) : base(config, logger) { }
  }
}
