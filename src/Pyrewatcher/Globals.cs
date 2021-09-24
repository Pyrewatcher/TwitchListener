using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pyrewatcher
{
  public static class Globals
  {
    public static Dictionary<string, List<string>> BroadcasterViewers = new();

    public static string LocaleCode = "PL";
    public static IDictionary<string, string> Locale;
    public static IDictionary<long, string> LolRunes;
    public static IDictionary<long, string> LolChampions;

    public static List<Type> CommandTypes;
    public static List<Type> ActionTypes;

    static Globals()
    {
      CommandTypes = Assembly.GetExecutingAssembly()
                             .GetTypes()
                             .Where(x => x.IsClass)
                             .Where(x => x.Name.EndsWith("Command") && x.Name != "Command")
                             .ToList();

      ActionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass).Where(x => x.Name.EndsWith("Action")).ToList();
    }
  }
}
