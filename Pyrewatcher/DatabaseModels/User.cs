using System.ComponentModel;

namespace Pyrewatcher.DatabaseModels
{
  public class User
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Role { get; set; } = "User";

    [Description("ignore")]
    public bool IsAdministrator
    {
      get => Role == "Administrator";
    }
  }
}
