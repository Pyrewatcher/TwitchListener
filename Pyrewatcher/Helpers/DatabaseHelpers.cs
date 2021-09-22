using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.Helpers
{
  public class DatabaseHelpers
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly LolChampionRepository _lolChampions;
    private readonly RuneReforgedRepository _runesReforged;
    private readonly TwitchApiHelper _twitchApiHelper;
    private readonly UserRepository _users;

    public DatabaseHelpers(BroadcasterRepository broadcasters, UserRepository users, RuneReforgedRepository runesReforged,
                           TwitchApiHelper twitchApiHelper, LolChampionRepository lolChampions)
    {
      _broadcasters = broadcasters;
      _users = users;
      _runesReforged = runesReforged;
      _twitchApiHelper = twitchApiHelper;
      _lolChampions = lolChampions;
    }

    public async Task<Broadcaster> GetBroadcaster(string broadcasterName)
    {
      var broadcaster = await _broadcasters.FindWithNameByNameAsync(broadcasterName);

      if (broadcaster != null)
      {
        return broadcaster;
      }

      var user = await _users.FindAsync("Name = @Name", new User {Name = broadcasterName.ToLower()});

      if (user == null)
      {
        user = await _twitchApiHelper.GetUserByName(broadcasterName);

        if (user.Id is 0 or -1)
        {
          return null;
        }

        await _users.InsertAsync(user);
      }

      broadcaster = new Broadcaster {Id = user.Id, DisplayName = user.DisplayName};
      await _broadcasters.InsertAsync(broadcaster);

      return broadcaster;
    }

    public async Task<Dictionary<long, string>> LoadRunes()
    {
      var runes = (await _runesReforged.FindAllAsync()).ToDictionary(rune => rune.Id, rune => rune.Name);

      return runes;
    }

    public async Task<Dictionary<long, string>> LoadLolChampions()
    {
      var lolChampions = (await _lolChampions.FindAllAsync()).ToDictionary(champion => champion.Id, champion => champion.Name);

      return lolChampions;
    }
  }
}
