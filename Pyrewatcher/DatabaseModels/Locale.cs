using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Locale
  {
    [Description("autoincrement")] public long Id { get; set; }
    public string Code { get; set; }
  }
}
