using System.Collections.Generic;

namespace SignalRTwitterDemo.Hubs
{
    public static class StreamHandler
    {
        public static Dictionary<string,List<string>> queriesToUsers = new Dictionary<string, List<string>>();
    }
}