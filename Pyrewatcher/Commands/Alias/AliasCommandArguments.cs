namespace Pyrewatcher.Commands
{
  public class AliasCommandArguments : ICommandArguments
  {
    public string Action { get; set; }
    public string Command { get; set; }
    public string Broadcaster { get; set; }
    public string Alias { get; set; }
  }
}
