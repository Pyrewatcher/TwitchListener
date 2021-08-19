namespace Pyrewatcher.Commands
{
  public class MessageCommandArguments : ICommandArguments
  {
    public string Broadcaster { get; set; }
    public string Message { get; set; }
  }
}
