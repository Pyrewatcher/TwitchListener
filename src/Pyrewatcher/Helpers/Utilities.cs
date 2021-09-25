namespace Pyrewatcher.Helpers
{
  public class Utilities
  {
    public string GetServerApiCode(string serverCode)
    {
      return serverCode.ToUpper() switch
      {
        "EUNE" => "eun1",
        "EUW" => "euw1",
        _ => null
      };
    }
  }
}
