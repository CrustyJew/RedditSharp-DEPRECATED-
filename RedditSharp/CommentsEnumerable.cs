using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedditSharp
{
    /// <summary>
    /// <see cref="IAsyncEnumerator{T}"/> for enumarting all comments on a post.
    /// Will traverse <see cref="More"/> objects it encounters.
    /// </summary>
    public class CommentsEnumarable : IAsyncEnumerable<Comment>
    {
        private Post post;
        private IWebAgent agent;
        private int limit;

        /// <summary>
        /// Constructs an <see cref="IAsyncEnumerable{T}"/> for the <see cref="Comment"/>(s) on the <paramref name="post"/>.
        /// This will result in multiple requests for larger comment trees as it will resolve all <see cref="More"/> objects
        /// it encounters.
        /// </summary>
        /// <param name="agent"> WebAgent necessary for requests</param>
        /// <param name="post">The <see cref="Post"/> of the comments section to enumerate</param>
        /// <param name="limitPerRequest">Initial request size, ignored by the MoreChildren endpoint</param>
        public CommentsEnumarable(IWebAgent agent, Post post, int limitPerRequest = 0)
        {
            this.post = post;
            this.agent = agent;
            limit = limitPerRequest;
        }
        /// <summary>
        /// Returns <see cref="IAsyncEnumerator{T}"/> for the comments on the <see cref="Post"/>> 
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerator<Comment> GetEnumerator()
        {
            return new CommentsEnumerator(agent, post, limit);
        }

        private class CommentsEnumerator : IAsyncEnumerator<Comment>
        {
            private const string GetCommentsUrl = "/comments/{0}.json";

            private Post post;
            private IWebAgent agent;
            private int limit;
            private List<More> existingMores;
            private IReadOnlyList<Comment> currentBranch;
            private int currentIndex;

            public CommentsEnumerator(IWebAgent agent, Post post, int limitPerRequest = 0)
            {
                existingMores = new List<Things.More>();
                currentIndex = -1;
                this.post = post;
                this.agent = agent;
                limit = limitPerRequest;
            }

            public Comment Current
            {
                get
                {
                    return currentBranch[currentIndex];
                }
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {

                if (currentIndex == -1)
                {
                    currentIndex = 0;
                    await GetBaseComments();
                    return currentBranch.Count > 0;
                }
                currentIndex++;
                if (currentIndex >= currentBranch.Count)
                {
                    if (existingMores.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        currentIndex = 0;
                        while (existingMores.Count > 0)
                        {
                            var more = existingMores.First();
                            existingMores.Remove(more);
                            List<Comment> newBranch = new List<Comment>();
                            List<Thing> newThings = await more.GetThingsAsync();
                            foreach (var thing in newThings)
                            {
                                if (thing.Kind == "more")
                                {
                                    existingMores.Add((More)thing);
                                }
                                else
                                {
                                    newBranch.Add((Comment)thing);
                                }
                            }
                            currentBranch = newBranch;
                            if (currentBranch.Count > 0) return true;
                        }
                        return false; //ran out of branches to check
                    }
                }
                return true;
            }

            private async Task GetBaseComments()
            {
                var url = string.Format(GetCommentsUrl, post.Id);
                if (limit > 0)
                {
                    var query = "limit=" + limit;
                    url = string.Format("{0}?{1}", url, query);
                }
                var json = await agent.Get(url).ConfigureAwait(false);
                var postJson = json.Last()["data"]["children"];

                List<Comment> retrieved = new List<Things.Comment>();
                foreach (var item in postJson)
                {
                    Comment newComment = new Comment(agent, item, post);
                    if (newComment.Kind != "more")
                    {
                        retrieved.Add(newComment);
                    }
                    else
                    {
                        existingMores.Add(new More(agent, item));
                    }
                }
                currentBranch = retrieved;
            }

            public void Dispose()
            {
                
            }
        }
    }
}
