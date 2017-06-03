using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using Tweetinvi.Core.Interfaces.Models;

namespace SignalRTwitterDemo.Hubs
{
    public class TwitterTaskData
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public Tweet Tweet { get; set; }
        [JsonIgnore]
        public CancellationTokenSource CancelToken { get; set; }

        public ConcurrentBag<string> Connections { get; set; }
    }
}