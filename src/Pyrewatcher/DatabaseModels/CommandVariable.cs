using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class CommandVariable
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long CommandId { get; set; }
    public string Name { get; set; }
    public string Value { get; set; } = "";
  }
}
