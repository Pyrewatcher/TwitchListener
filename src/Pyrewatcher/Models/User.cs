namespace Pyrewatcher.Models
{
  public class User
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Role { get; set; }
    
    public bool IsAdministrator
    {
      get => Role == "Administrator";
    }

    private User()
    {
      // needed by Dapper
    }

    public User(long id, string displayName, string role = "User")
    {
      Id = id;
      Name = displayName?.ToLower();
      DisplayName = displayName;
      Role = role;
    }
  }
}
