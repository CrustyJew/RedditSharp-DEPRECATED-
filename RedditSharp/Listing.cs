using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedditSharp
{
    public enum Sorting
    {
        Relevance,
        New,
        Top,
        Comments
    }

    public enum TimeSorting
    {
        All,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public class ListingStream<T> : IObservable<T> where T : Thing {

        Listing<T> Listing { get; set; }
        List<IObserver<T>> _observers;

        internal ListingStream(Listing<T> listing) {
            Listing = listing;
            _observers = new List<IObserver<T>>();
        }

        public IDisposable Subscribe(IObserver<T> observer) {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        public async Task Enumerate() {
            await Listing.ForEachAsync(page => {
                  foreach(var thing in page) {
                      foreach(var observer in _observers) {
                          observer.OnNext(thing);
                      }
                  }
                });
        }

        private class Unsubscriber : IDisposable {

          private ICollection<IObserver<T>> _observers;
          private IObserver<T> _observer;

          public Unsubscriber(ICollection<IObserver<T>> observers,
                              IObserver<T> observer) {
              _observers = observers;
              _observer = observer;
          }

          public void Dispose() {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
          }

        }

    }

    public class Listing<T> : RedditObject, IAsyncEnumerable<IReadOnlyCollection<T>> where T : Thing
    {
        /// <summary>
        /// Gets the default number of listings returned per request
        /// </summary>
        internal const int DefaultListingPerRequest = 25;

        private string Url { get; }

        /// <summary>
        /// Creates a new Listing instance
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="url"></param>
        /// <param name="webAgent"></param>
        internal Listing(Reddit reddit, string url) : base(reddit)
        {
            Url = url;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection, using the specified number of listings per
        /// request and optionally the maximum number of listings
        /// </summary>
        /// <param name="limitPerRequest">The number of listings to be returned per request</param>
        /// <param name="maximumLimit">The maximum number of listings to return</param>
        /// <returns></returns>
        public IAsyncEnumerator<IReadOnlyCollection<T>> GetEnumerator(int limitPerRequest, int maximumLimit = -1, bool stream = false)
        {
            return new ListingEnumerator(this, limitPerRequest, maximumLimit, stream);
        }

        public IAsyncEnumerator<IReadOnlyCollection<T>> GetEnumerator()
        {
            return GetEnumerator(DefaultListingPerRequest);
        }

        public ListingStream<T> GetListingStream() {
            return new ListingStream<T>(this);
        }

#pragma warning disable 0693
        private class ListingEnumerator : IAsyncEnumerator<IReadOnlyCollection<T>>
        {
            private bool stream = false;
            private Listing<T> Listing { get; set; }
            private string After { get; set; }
            private string Before { get; set; }
            private ReadOnlyCollection<T> CurrentPage { get; set; }
            private int Count { get; set; }
            private int LimitPerRequest { get; set; }
            private int MaximumLimit { get; set; }

            private ICollection<string> done;

            /// <summary>
            /// Creates a new ListingEnumerator instance
            /// </summary>
            /// <param name="listing"></param>
            /// <param name="limitPerRequest">The number of listings to be returned per request. -1 will exclude this parameter and use the Reddit default (25)</param>
            /// <param name="maximumLimit">The maximum number of listings to return, -1 will not add a limit</param>
            /// <param name="stream">yield new <see cref="Thing"/> as they are created</param>
            public ListingEnumerator(Listing<T> listing, int limitPerRequest, int maximumLimit, bool stream = false)
            {
                Listing = listing;
                CurrentPage = new ReadOnlyCollection<T>(new T[0]);
                done = new HashSet<string>();
                this.stream = stream;

                // Set the listings per page (if not specified, use the Reddit default of 25) and the maximum listings
                LimitPerRequest = (limitPerRequest <= 0 ? DefaultListingPerRequest : limitPerRequest);
                MaximumLimit = maximumLimit;
            }

            public IReadOnlyCollection<T> Current => CurrentPage;

            private Task FetchNextPageAsync()
            {
                if (stream)
                    return PageForwardAsync();
                else
                    return PageBackAsync();
            }

            string AppendQueryParam(string url, string param, string value) =>
                url + (url.Contains("?") ? "&" : "?") + param + "=" + value;

            string AppendCommonParams(string url) {
                if (LimitPerRequest < 0)
                {
                    int limit = LimitPerRequest;
                    if(MaximumLimit < 0)
                    {
                        limit = new [] {LimitPerRequest, MaximumLimit, Count + LimitPerRequest - MaximumLimit}.Min();
                    }
                    if (limit > 0)
                    {
                        // Add the limit, the maximum number of items to be returned per page
                        url = AppendQueryParam(url, "limit", limit.ToString());
                    }
                }

                if (Count > 0)
                {
                    // Add the count, the number of items already seen in this listing
                    // The Reddit API uses this to determine when to give values for before and after fields
                    url = AppendQueryParam(url, "count", Count.ToString());
                }
                return url;
            }

            /// <summary>
            /// Standard behavior.  Page from newest to oldest - "backward" in time.
            /// </summary>
            private async Task PageBackAsync()
            {
                var url = Listing.Url;

                if (After != null)
                {
                    url = AppendQueryParam(url, "after", After);
                }
                url = AppendCommonParams(url);
                var json = await Listing.WebAgent.Get(url).ConfigureAwait(false);
                if (json["kind"].ValueOrDefault<string>() != "Listing")
                    throw new FormatException("Reddit responded with an object that is not a listing.");
                Parse(json);
            }

            /// <summary>
            /// Page from oldest to newest - "forward" in time.
            /// </summary>
            private async Task PageForwardAsync()
            {
                var url = Listing.Url;

                if (Before != null)
                {
                    url = AppendQueryParam(url, "before", Before);
                }
                url = AppendCommonParams(url);
                var json = await Listing.WebAgent.Get(url).ConfigureAwait(false);
                if (json["kind"].ValueOrDefault<string>() != "Listing")
                    throw new FormatException("Reddit responded with an object that is not a listingStream.");
                Parse(json);
            }

            private void Parse(JToken json)
            {
                var children = json["data"]["children"] as JArray;
                var things = new List<T>();

                for (int i = 0; i < children.Count; i++)
                {
                    if (!stream)
                        things.Add(Thing.Parse<T>(Listing.Reddit, children[i]));
                    else
                    {
                        var kind = children[i]["kind"].ValueOrDefault<string>();
                        var id = children[i]["data"]["id"].ValueOrDefault<string>();

                        // check for new replies to pm / modmail
                        if (kind == "t4" && children[i]["data"]["replies"].HasValues)
                        {
                            var replies = children[i]["data"]["replies"]["data"]["children"] as JArray;
                            foreach (var reply in replies)
                            {
                                var replyId = reply["data"]["id"].ValueOrDefault<string>();
                                if (done.Contains(replyId))
                                    continue;

                                things.Add(Thing.Parse<T>(Listing.Reddit, reply));
                                done.Add(replyId);
                            }
                        }

                        if (String.IsNullOrEmpty(id) || done.Contains(id))
                            continue;

                        things.Add(Thing.Parse<T>(Listing.Reddit, children[i]));
                        done.Add(id);
                    }
                }

                // this doesn't really work when we're processing messages with replies.
                if (stream)
                    things.Reverse();

                CurrentPage = new ReadOnlyCollection<T>(things);
                // Increase the total count of items returned
                Count += CurrentPage.Count;

                After = json["data"]["after"].Value<string>();
                Before = json["data"]["before"].Value<string>();
            }

            public void Dispose()
            {
                // ...
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (stream) {
                    return await MoveNextForwardAsync().ConfigureAwait(false);
                } else {
                    return await MoveNextBackAsync().ConfigureAwait(false);
                }
            }

            private async Task<bool> MoveNextBackAsync()
            {
                if (After == null)
                {
                    // No more pages to return
                    return false;
                }

                if (MaximumLimit != -1 && Count >= MaximumLimit)
                {
                    // Maximum listing count returned
                    return false;
                }

                // Get the next page
                await FetchNextPageAsync().ConfigureAwait(false);

                if (CurrentPage.Count == 0)
                {
                    // No listings were returned in the page
                    return false;
                }
                return true;
            }

            private async Task<bool> MoveNextForwardAsync()
            {
                int tries = 0;
                while (true)
                {
                    if (MaximumLimit != -1 && Count >= MaximumLimit)
                        return false;

                    tries++;
                    // Get the next page
                    try
                    {
                        await FetchNextPageAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // sleep for a while to see if we can recover
                        await Sleep(tries, ex).ConfigureAwait(false);
                    }

                    if (CurrentPage.Count == 0)
                    {
                        // No listings were returned in the page
                        // sleep for a while
                        await Sleep(tries).ConfigureAwait(false);
                    }
                    else
                    {
                        tries = 0;
                        break;
                    }
                }
                return true;
            }

            private async Task Sleep(int tries, Exception ex = null)
            {
                // wait up to 3 minutes between tries
                // TODO: Make this configurable
                int seconds = 180;

                // TODO: Make this configurable
                if (tries > 36)
                {
                    if (ex != null)
                        throw ex;
                }
                else
                {
                    seconds = tries*5;
                }
                await Task.Delay(seconds * 1000).ConfigureAwait(false);
            }
        }
#pragma warning restore
    }
}
