using System.Threading.Tasks;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DatabaseModels;

namespace Pyrewatcher.Helpers
{
  public class DatabaseHelpers
  {
    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly UserRepository _usersRepository;

    private readonly TwitchApiHelper _twitchApiHelper;

    public DatabaseHelpers(IBroadcastersRepository broadcastersRepository, UserRepository usersRepository, TwitchApiHelper twitchApiHelper)
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

      var user = await _usersRepository.FindAsync("Name = @Name", new User {Name = broadcasterName.ToLower()});

      if (user == null)
      {
        user = await _twitchApiHelper.GetUserByName(broadcasterName);

        if (user.Id is 0 or -1)
        {
          return null;
        }

        await _usersRepository.InsertAsync(user);
      }

      var inserted = await _broadcastersRepository.InsertAsync(user.Id);

      if (inserted)
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
