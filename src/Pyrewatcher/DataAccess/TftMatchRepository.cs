using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class TftMatchRepository : Repository<TftMatch>
  {
    public override string TableName
    {
      get => "TftMatches";
    }

    public TftMatchRepository(IConfiguration config, ILogger<Repository<TftMatch>> logger) : base(config, logger) { }

    public async Task<int> InsertRangeIfNotExistsAsync(IEnumerable<TftMatch> list)
    {
      var inserted = 0;

      foreach (var tftMatch in list)
      {
        if (await FindAsync("MatchId = @MatchId AND AccountId = @AccountId", tftMatch) != null)
        {
          continue;
        }

        await InsertAsync(tftMatch);
        inserted++;
      }

      return inserted;
    }
  }
}
