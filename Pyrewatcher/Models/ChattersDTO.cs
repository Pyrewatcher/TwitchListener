using System.Collections.Generic;

namespace Pyrewatcher.Models
{
  public class ChattersDTO
  {
    public List<string> Broadcaster { get; set; }
    public List<string> Vips { get; set; }
    public List<string> Moderators { get; set; }
    public List<string> Staff { get; set; }
    public List<string> Admins { get; set; }
    public List<string> Global_Mods { get; set; }
    public List<string> Viewers { get; set; }
  }
}
