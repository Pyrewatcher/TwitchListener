using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ICommandVariablesRepository
  {
    Task<string> GetCommandTextById(long commandId);
  }
}