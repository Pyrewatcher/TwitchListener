using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface IAdoZRepository
  {
    Task<IEnumerable<AdoZEntry>> GetAllEntriesAsync();
  }
}