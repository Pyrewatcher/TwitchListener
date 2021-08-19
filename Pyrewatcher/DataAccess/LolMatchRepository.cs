using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.DataAccess
{
  public class LolMatchRepository : Repository<LolMatch>
  {
    public override string TableName
    {
      get => "LolMatches";
    }

    public LolMatchRepository(IConfiguration config, ILogger<Repository<LolMatch>> logger) : base(config, logger) { }

    public async Task<int> InsertRangeIfNotExistsAsync(IEnumerable<LolMatch> list)
    {
      var inserted = 0;

      foreach (var lolMatch in list)
      {
        if (await FindAsync("MatchId = @MatchId AND ServerApiCode = @ServerApiCode AND AccountId = @AccountId", lolMatch) != null)
        {
          continue;
        }

        await InsertAsync(lolMatch);
        inserted++;
      }

      return inserted;
    }
  }
}
