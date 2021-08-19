using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class UserRepository : Repository<User>
  {
    public override string TableName
    {
      get => "Users";
    }

    public UserRepository(IConfiguration config, ILogger<Repository<User>> logger) : base(config, logger) { }
  }
}
