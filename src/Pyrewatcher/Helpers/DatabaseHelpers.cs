using System.Threading.Tasks;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;

namespace Pyrewatcher.Helpers
{
  public class DatabaseHelpers
  {
    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly IUsersRepository _usersRepository;

    private readonly TwitchApiHelper _twitchApiHelper;

    public DatabaseHelpers(IBroadcastersRepository broadcastersRepository, IUsersRepository usersRepository, TwitchApiHelper twitchApiHelper)
    {
      _broadcastersRepository = broadcastersRepository;
      _usersRepository = usersRepository;
      _twitchApiHelper = twitchApiHelper;
    }

    public async Task<Broadcaster> GetBroadcaster(string broadcasterName)
    {
      var broadcaster = await _broadcastersRepository.GetByNameAsync(broadcasterName);

      if (broadcaster != null)
      {
        return broadcaster;
      }

      //var user = await _usersRepository.FindAsync("Name = @Name", new User {Name = broadcasterName.ToLower()});
      var user = await _usersRepository.GetUserByName(broadcasterName);

      if (user is null)
      {
        user = await _twitchApiHelper.GetUserByName(broadcasterName);

        if (user.Id is 0 or -1)
        {
          return null;
        }

        var userInserted = await _usersRepository.InsertUser(user);

        if (!userInserted)
        {
          // TODO: Log failure
        }
      }

      var broadcasterInserted = await _broadcastersRepository.InsertAsync(user.Id);

      if (broadcasterInserted)
      {
        broadcaster = await _broadcastersRepository.GetByNameAsync(broadcasterName);

        return broadcaster;
      }
      else
      {
        // TODO: Log failure
        return null;
      }
    }
  }
}
