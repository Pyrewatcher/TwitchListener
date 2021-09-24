using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pyrewatcher.Actions
{
  public interface IAction
  {
    public string MsgId { get; }
    public Task PerformAsync(Dictionary<string, string> args);
  }
}
