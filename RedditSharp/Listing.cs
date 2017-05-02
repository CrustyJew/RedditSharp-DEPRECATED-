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
#pragma warning disable 1591
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
#pragma warning restore 1591

    /// <summary>
    /// A semi-realtime stream of <see cref="Thing"/> being posted to an item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListingStream<T> : IObservable<T> where T : Thing
    {

        Listing<T> Listing { get; set; }
        List<IObserver<T>> _observers;

        internal ListingStream(Listing<T> listing)
        {
            Listing = listing;
            Listing.IsStream = true;
            _observers = new List<IObserver<T>>();
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

#pragma warning disable 1591
        public async Task Enumerate()
        {
            await Listing.ForEachAsync(thing =>
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(thing);
                }

            });
        }
#pragma warning restore 1591

        private class Unsubscriber : IDisposable
        {

            private ICollection<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(ICollection<IObserver<T>> observers,
                                IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }

        }

    }

    /// <summary>
    /// A reddit listing.  https://github.com/reddit/reddit/wiki/JSON#listing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Listing<T> : RedditObject, IAsyncEnumerable<T> where T : Thing
    {
        /// <summary>
        /// Gets the default number of listings returned per request
        /// </summary>
        internal const int DefaultListingPerRequest = 25;

        private string Url { get; }

        /// <summary>
        /// Creates a new Listing instance
        /// </summary>
        /// <param name="agent">IWebAgent to use for requests</param>
        /// <param name="url">Endpoint</param>
        /// <param name="maxLimit">Maximum number of records to retrieve from reddit.</param>
        /// <param name="limitPerRequest">Maximum number of records to return per request.  This number is endpoint specific.</param>
        internal Listing(IWebAgent agent, string url, int maxLimit = -1, int limitPerRequest = -1) : base(agent)
        {
            LimitPerRequest = limitPerRequest;
            MaximumLimit = maxLimit;
            IsStream = false;
            Url = url;
        }

        /// <summary>
        /// Create a listing with the specified limits. 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="url">Endpoint</param>
        /// <param name="max">Maximum number of records to retrieve from reddit.</param>
        /// <param name="perRequest">Maximum number of records to return per request.  This number is endpoint specific.</param>
        /// <returns></returns>
        internal static Listing<T> Create(IWebAgent agent, string url, int max, int perRequest)
        {
            if (max > 0 && max <= perRequest)
                perRequest = max;

            return new Listing<T>(agent, url, max, perRequest);
        }

        /// <summary>
        /// Number of records to return for each request.
        /// </summary>
        public int LimitPerRequest { get; set; }

        /// <summary>
        /// Maximum number of records to return.
        /// </summary>
        public int MaximumLimit { get; set; }

        /// <summary>
        /// Returns true is this a ListingStream.
        /// </summary>
        internal bool IsStream { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through a collection, using the specified number of listings per
        /// request and optionally the maximum number of listings
        /// </summary>
        /// <param name="limitPerRequest">The number of listings to be returned per request</param>
        /// <param name="maximumLimit">The maximum number of listings to return</param>
        /// <param name="stream">Set to true for a listing stream.</param>
        /// <returns></returns>
        public IAsyncEnumerator<T> GetEnumerator(int limitPerRequest, int maximumLimit = -1, bool stream = false)
        {
            return new ListingEnumerator(this, limitPerRequest, maximumLimit, stream);
        }

        /// <inheritdoc/>
        public IAsyncEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(LimitPerRequest,MaximumLimit,IsStream);
        }

        /// <summary>
        /// Poll the listing for new items.
        /// </summary>
        /// <returns></returns>
        public ListingStream<T> Stream()
        {
            return new ListingStream<T>(this);
        }

#pragma warning disable 0693
        private class ListingEnumerator : IAsyncEnumerator<T>
        {
            private bool stream = false;
            private Listing<T> Listing { get; set; }
            private string After { get; set; }
            private string Before { get; set; }
            private ReadOnlyCollection<T> CurrentPage { get; set; }
            private int CurrentIndex { get; set; }
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
                CurrentPage = null;// new ReadOnlyCollection<T>(new T[0]);
                CurrentIndex = -1;
                done = new HashSet<string>();
                this.stream = stream;

                // Set the listings per page (if not specified, use the Reddit default of 25) and the maximum listings
                LimitPerRequest = (limitPerRequest <= 0 ? DefaultListingPerRequest : limitPerRequest);
                MaximumLimit = maximumLimit;
            }

            public T Current => CurrentPage.ElementAtOrDefault(CurrentIndex);

            private Task FetchNextPageAsync()
            {
                if (stream)
                    return PageForwardAsync();
                else
                    return PageBackAsync();
            }

            string AppendQueryParam(string url, string param, string value) =>
                url + (url.Contains("?") ? "&" : "?") + param + "=" + value;

            string AppendCommonParams(string url)
            {
                if (LimitPerRequest > 0)
                {
                    int limit = LimitPerRequest;
                    if (MaximumLimit < 0)
                    {
                        limit = new[] { LimitPerRequest, MaximumLimit, Count + LimitPerRequest - MaximumLimit }.Min();
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
                //json = json.Last();
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
                        things.Add(Thing.Parse<T>(Listing.WebAgent, children[i]));
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

                                things.Add(Thing.Parse<T>(Listing.WebAgent, reply));
                                done.Add(replyId);
                            }
                        }

                        if (String.IsNullOrEmpty(id) || done.Contains(id))
                            continue;

                        things.Add(Thing.Parse<T>(Listing.WebAgent, children[i]));
                        done.Add(id);
                    }
                }

                // this doesn't really work when we're processing messages with replies.
                if (stream)
                    things.Reverse();

                CurrentPage = new ReadOnlyCollection<T>(things);
                // Increase the total count of items returned


                After = json["data"]["after"].Value<string>();
                Before = json["data"]["before"].Value<string>();
            }

            public void Dispose()
            {
                // ...
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (stream)
                {
                    return await MoveNextForwardAsync().ConfigureAwait(false);
                }
                else
                {
                    return await MoveNextBackAsync().ConfigureAwait(false);
                }
            }

            private async Task<bool> MoveNextBackAsync()
            {
                if (CurrentIndex == -1)
                {
                    //first call, get a page and set CurrentIndex
                    await FetchNextPageAsync().ConfigureAwait(false);
                    CurrentIndex = 0;
                    Count = 1;
                    return CurrentPage.Count > 0; //if there are no results, return false
                }
                else
                {
                    Count++;
                    CurrentIndex++;
                }
                //I don't think we want to use Count here. Look into this.
                if (MaximumLimit != -1 && Count >= MaximumLimit)
                {
                    // Maximum listing count returned
                    return false;
                }
                if (CurrentIndex >= CurrentPage.Count)
                {
                    if (After == null)
                    {
                        // No more pages to return
                        return false;
                    }
                    else
                    {
                        await FetchNextPageAsync().ConfigureAwait(false);
                        CurrentIndex = 0;
                        return CurrentPage.Count > 0; //if there are no results, return false
                    }
                }

                return true;
            }

            private async Task<bool> MoveNextForwardAsync()
            {
                CurrentIndex++;
                int tries = 0;
                while (true)
                {
                    tries++;

                    if (MaximumLimit != -1 && Count >= MaximumLimit)
                        return false;

                    try
                    {
                        await FetchNextPageAsync().ConfigureAwait(false);
                        CurrentIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        // sleep for a while to see if we can recover
                        await Sleep(tries, ex).ConfigureAwait(false);
                    }

                    // the page is only populated if there are *new* items to yielded from the listing.
                    if (CurrentPage.Count > 0)
                        break;

                    // No listings were returned in the page.
                    await Sleep(tries).ConfigureAwait(false);
                }
                Count++;
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
                    seconds = tries * 5;
                }
                await Task.Delay(seconds * 1000).ConfigureAwait(false);
            }
        }
#pragma warning restore
    }
}
