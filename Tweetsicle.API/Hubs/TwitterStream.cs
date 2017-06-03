using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Tweetinvi;
using Tweetinvi.Core.Interfaces.Streaminvi;

namespace SignalRTwitterDemo.Hubs
{
    public static class TwitterStream
    {
        private static Dictionary<string,IFilteredStream> _streams = new Dictionary<string, IFilteredStream>();
        private static readonly IHubContext _context = GlobalHost.ConnectionManager.GetHubContext<TwitterHub>();
        public static async Task StartStream(string query, CancellationToken token)
        {

            Auth.SetUserCredentials(
                TwitterCredentials.CONSUMER_KEY, 
                TwitterCredentials.CONSUMER_SECRET, 
                TwitterCredentials.ACCESS_TOKEN, 
                TwitterCredentials.ACCESS_TOKEN_SECRET);
            if (_streams.ContainsKey(query))
            {
                _streams[query].ResumeStream();
            }
            else
            {
                _streams.Add(query, Stream.CreateFilteredStream());
                _streams[query].MatchOn = MatchOn.HashTagEntities;

                var searchTerms = query.Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    _streams[query].AddTrack(searchTerm.Replace("#",""));
                }
                _streams[query].MatchingTweetReceived += async (sender, args) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        _streams[query].StopStream();
                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        var embedTweet = Tweetinvi.Tweet.GenerateOEmbedTweet(args.Tweet);
                        var generatedHtml = embedTweet == null ? "<div class='twitter-tweet'>" + args.Tweet.ToString() + "</div>" : embedTweet.HTML;
                        await _context.Clients.Group(query).updateTweet(new Tweet(args.Tweet, generatedHtml));
                    }
                };
                _streams[query].StreamPaused += async (sender, args) => { await _context.Clients.Group(query).updateStatus("Paused."); };
                _streams[query].StreamResumed += async (sender, args) => { await _context.Clients.Group(query).updateStatus("Streaming..."); };
                _streams[query].StreamStarted += async (sender, args) => { await _context.Clients.Group(query).updateStatus("Started."); };
                _streams[query].StreamStopped += async (sender, args) => { await _context.Clients.Group(query).updateStatus("Stopped (event)"); };

                await _streams[query].StartStreamMatchingAnyConditionAsync();
            }
            await _context.Clients.Group(query).updateStatus("Started.");
        }
    }
}
