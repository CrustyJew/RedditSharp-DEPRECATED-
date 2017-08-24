using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A thing that can be voted on or actionable by a moderator.
    /// </summary>
    public class VotableThing : ModeratableThing
    {
#pragma warning disable 1591
        public VotableThing(IWebAgent agent, JToken json) : base(agent, json) {
        }

        public enum VoteType
        {
            Upvote = 1,
            None = 0,
            Downvote = -1
        }

#pragma warning restore 1591

        private const string VoteUrl = "/api/vote";
        private const string SaveUrl = "/api/save";
        private const string UnsaveUrl = "/api/unsave";

        private const string DelUrl = "/api/del";

        /// <summary>
        /// Css flair class of the item author.
        /// </summary>
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; private set; }

        /// <summary>
        /// Flair text of the item author.
        /// </summary>
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; private set; }

        /// <summary>
        /// Number of upvotes on this item.
        /// </summary>
        [JsonProperty("downs")]
        public int Downvotes { get; private set; }

        /// <summary>
        /// Returns true if this item has been edited by the author.
        /// </summary>
        [JsonProperty("edited")]
        public bool Edited { get; private set; }

        /// <summary>
        /// Returns true if this item is archived.
        /// </summary>
        [JsonProperty("archived")]
        public bool IsArchived { get; private set; }
 
        /// <summary>
        /// Number of upvotes on this item.
        /// </summary>
        [JsonProperty("ups")]
        public int Upvotes { get; private set; }

        /// <summary>
        /// Returns true if this item is saved.
        /// </summary>
        [JsonProperty("saved")]
        public bool Saved { get; private set; }

        /// <summary>
        /// Shortlink to the item
        /// </summary>
        public virtual string Shortlink => "http://redd.it/" + Id;

        /// <summary>
        /// Returns true if the item is sticked.
        /// </summary>
        [JsonProperty("stickied")]
        public bool IsStickied { get; private set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not.
        /// Null if they have not cast a vote.
        /// </summary>
        [JsonProperty("likes")]
        public bool? Liked { get; private set; }

        /// <summary>
        /// Number of times this item has been gilded.
        /// </summary>
        [JsonProperty("gilded")]
        public int Gilded { get; private set; }

        /// <summary>
        /// Gets or sets the vote for the current VotableThing.
        /// </summary>
        [JsonIgnore]
        public VoteType Vote
        {
            get
            {
                switch (this.Liked)
                {
                    case true: return VoteType.Upvote;
                    case false: return VoteType.Downvote;
                    default: return VoteType.None;
                }
            }
            private set
            {
                Task.Run(async () => { await SetVoteAsync(value).ConfigureAwait(false); });
            }
        }
        /// <summary>
        /// Upvotes something
        /// </summary>
        public Task UpvoteAsync()
        {
            return this.SetVoteAsync(VoteType.Upvote);
        }

        /// <summary>
        /// Downvote this item.
        /// </summary>
        public Task DownvoteAsync()
        {
            return this.SetVoteAsync(VoteType.Downvote);
        }

        /// <summary>
        /// Vote on this item.
        /// </summary>
        /// <param name="type"></param>
        public async Task SetVoteAsync(VoteType type)
        {
            if (this.Vote == type) return;

            var data = await WebAgent.Post(VoteUrl, new
            {
                dir = (int)type,
                id = FullName
            }).ConfigureAwait(false);

            if (Liked == true) Upvotes--;
            if (Liked == false) Downvotes--;

            switch (type)
            {
                case VoteType.Upvote: Liked = true; Upvotes++; return;
                case VoteType.None: Liked = null; return;
                case VoteType.Downvote: Liked = false; Downvotes++; return;
            }
        }

        /// <summary>
        /// Save this item.
        /// </summary>
        public async Task SaveAsync()
        {
            await WebAgent.Post(SaveUrl, new
            {
                id = FullName
            }).ConfigureAwait(false);
            Saved = true;
        }

        /// <summary>
        /// Unsave this item.
        /// </summary>
        public async Task UnsaveAsync()
        {
            await WebAgent.Post(UnsaveUrl, new
            {
                id = FullName
            }).ConfigureAwait(false);
            Saved = false;
        }

        //TODO clean this up, unnecessary calls
        /// <summary>
        /// Clear you vote on this item.
        /// </summary>
        public async Task ClearVote()
        {
            await WebAgent.Post(VoteUrl, new
            {
                dir = 0,
                id = FullName
            }).ConfigureAwait(false);
        }
    }
}
