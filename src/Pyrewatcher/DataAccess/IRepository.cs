using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess
{
  public interface IRepository<T>
  {
    Task DeleteAsync(string parameterizedWhereClause, T parameters);
    Task<T> FindAsync(string parameterizedWhereClause, T parameters);
    Task<IEnumerable<T>> FindAllAsync();
    Task<IEnumerable<T>> FindRangeAsync(string parameterizedWhereClause, T parameters);
    Task InsertAsync(T t);
    Task<int> InsertRangeAsync(IEnumerable<T> list);
    Task UpdateAsync(T t);
  }
}
