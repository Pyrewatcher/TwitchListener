using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class LocaleLine
  {
    [Description("autoincrement")] public long Id { get; set; }
    public string Name { get; set; }
    public string LocaleCode { get; set; }
    public string Line { get; set; }
  }
}
