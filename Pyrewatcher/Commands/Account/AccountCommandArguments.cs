namespace Pyrewatcher.Commands
{
  public class AccountCommandArguments : ICommandArguments
  {
    public string Action { get; set; }
    public string Broadcaster { get; set; }
    public string Game { get; set; }
    public string Server { get; set; }
    public string SummonerName { get; set; }
    public long AccountId { get; set; }
    public string NewDisplayName { get; set; }
  }
}
