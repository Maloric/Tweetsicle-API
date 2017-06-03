using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Tweetinvi.Core.Extensions;

namespace SignalRTwitterDemo.Hubs
{
    [HubName("twitterHub")]
    public class TwitterHub : Hub
    {
        private static ConcurrentDictionary<string, TwitterTaskData> _currentTasks;

        private static ConcurrentDictionary<string, TwitterTaskData> CurrentTasks => _currentTasks ?? (_currentTasks = new ConcurrentDictionary<string, TwitterTaskData>());

        public async Task Register(string query)
        {
            if (StreamHandler.queriesToUsers.ContainsKey(query))
            {
                StreamHandler.queriesToUsers[query].Add(Context.ConnectionId);
            }
            else
            {
                StreamHandler.queriesToUsers.Add(query, new List<string>() { Context.ConnectionId});
            }

            var tokenSource = new CancellationTokenSource();

            CurrentTasks.TryAdd(query, new TwitterTaskData
            {
                CancelToken = tokenSource,
                Id = query,
                Status = "Initializing."
            });

            await Groups.Add(Context.ConnectionId, query);

            try
            {

                var task = TwitterStream.StartStream(query, tokenSource.Token);

                await task;
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            { }
        }

        public async Task Unregister()
        {
            foreach (var stream in StreamHandler.queriesToUsers)
            {
                await Groups.Remove(Context.ConnectionId, stream.Key);
                if (stream.Value.Contains(Context.ConnectionId))
                {
                    stream.Value.Remove(Context.ConnectionId);
                    if (stream.Value.IsEmpty())
                    {
                        CurrentTasks[stream.Key].CancelToken.Cancel();
                    }
                }
            }

            await Clients.Caller.updateStatus("Stopped.");
        }
    }
}