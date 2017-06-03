using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Core.Interfaces;

namespace SignalRTwitterDemo.Hubs
{
    [Serializable]
    public class Tweet
    {
        public string id { get; set; }
        public IEnumerable<string> hashtags { get; set; }
        public string html { get; set; }

        public Tweet(ITweet data, string generatedHtml)
        {
            id = data.IdStr;
            hashtags = data.Hashtags.Select(h => h.Text);
            html = generatedHtml;
        }
    }
}