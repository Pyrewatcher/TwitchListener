using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILocalizationRepository
  {
    Task<IDictionary<string, string>> GetLocalizationByCodeAsync(string localeCode);
  }
}