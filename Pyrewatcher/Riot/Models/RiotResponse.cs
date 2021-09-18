using Pyrewatcher.Common.Interfaces;

namespace Pyrewatcher.Riot.Models
{
  public class RiotResponse<T> : IResponse<T>
  {
    public int StatusCode { get; set; }
    public T Content { get; set; }
    public string ErrorMessage { get; set; }

    public RiotResponse(int statusCode, T content, string errorMessage = null)
    {
      StatusCode = statusCode;
      Content = content;
      ErrorMessage = errorMessage;
    }
  }
}
