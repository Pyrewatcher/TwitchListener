using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class Ban
  {
    [Description("autoincrement")] public long Id { get; set; }
    public long UserId { get; set; }
  }
}
