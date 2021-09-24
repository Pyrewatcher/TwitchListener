using System.Threading.Tasks;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.Helpers
{
  public class DatabaseHelpers
  {
    private readonly BroadcasterRepository _broadcasters;
    private readonly TwitchApiHelper _twitchApiHelper;
    private readonly UserRepository _users;

    public DatabaseHelpers(BroadcasterRepository broadcasters, UserRepository users, TwitchApiHelper twitchApiHelper)
    {
      _broadcasters = broadcasters;
      _users = users;
      _twitchApiHelper = twitchApiHelper;
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
  }
}
