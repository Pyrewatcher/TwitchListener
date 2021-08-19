namespace Pyrewatcher.Commands
{
  public class CooldownCommandArguments : ICommandArguments
  {
    public string Command { get; set; }
    public int? NewValue { get; set; }
  }
}
