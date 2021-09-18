namespace Pyrewatcher.Common.Interfaces
{
  public interface IResponse<T>
  {
    public int StatusCode { get; set; }
    public T Content { get; set; }
    public string ErrorMessage { get; set; }

    bool IsSuccess
    {
      get => StatusCode is >= 200 and <= 299;
    }
  }
}
