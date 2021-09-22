using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILolChampionsRepository
  {
    Task<IDictionary<long, string>> GetAllAsync();
  }
}