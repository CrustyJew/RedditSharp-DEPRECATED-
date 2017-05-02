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
    public class VotableThing : CreatedThing
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

        public enum ReportType
        {
            Spam = 0,
            VoteManipulation = 1,
            PersonalInformation = 2,
            SexualizingMinors = 3,
            BreakingReddit = 4,
            Other = 5
        }

        public enum DistinguishType
        {
            Moderator,
            Admin,
            Special,
            None
        }
#pragma warning restore 1591

        private const string VoteUrl = "/api/vote";
        private const string SaveUrl = "/api/save";
        private const string UnsaveUrl = "/api/unsave";
        private const string ReportUrl = "/api/report";
        private const string DistinguishUrl = "/api/distinguish";

        private const string ApproveUrl = "/api/approve";
        private const string DelUrl = "/api/del";
        private const string RemoveUrl = "/api/remove";
        private const string IgnoreReportsUrl = "/api/ignore_reports";
        private const string UnIgnoreReportsUrl = "/api/unignore_reports";

        /// <summary>
        /// The moderator who approved this item.  This will be null or empty if the item has not been approved.
        /// </summary>
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; private set; }

        /// <summary>
        /// Author user name.
        /// </summary>
        [JsonProperty("author")]
        public string AuthorName { get; private set; }

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
        /// The moderator who removed this item.  This will be null or empty if the item has not been removed.
        /// </summary>
        [JsonProperty("banned_by")]
        public string BannedBy { get; private set; }

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
        /// Returns true if this item has been approved.
        /// Returns false if the item has not been approved.  A value of false does not indicate
        /// an item has been removed.
        ///
        /// <para>Returns null if the logged in user is not a moderator in the items subreddit.</para>
        /// </summary>
        [JsonProperty("approved")]
        public bool? IsApproved { get; private set; }

        /// <summary>
        /// Returns true if this item has been removed.
        /// Returns false if the item has not been removed.  A value of false does not indicate
        /// an item has been approved.
        ///
        /// <para>Returns null if the logged in user is not a moderator in the items subreddit.</para>
        /// </summary>
        [JsonProperty("removed")]
        public bool? IsRemoved { get; private set; }

        /// <summary>
        /// Number of upvotes on this item.
        /// </summary>
        [JsonProperty("ups")]
        public int Upvotes { get; private set; }

        /// <summary>
        /// Current score of this item.
        /// </summary>
        [JsonProperty("score")]
        public int Score { get; private set; }

        /// <summary>
        /// Returns true if this item is saved.
        /// </summary>
        [JsonProperty("saved")]
        public bool Saved { get; private set; }

        /// <summary>
        /// Returns true if the item is sticked.
        /// </summary>
        [JsonProperty("stickied")]
        public bool IsStickied { get; private set; }

        /// <summary>
        /// Number of reports on this item.
        /// </summary>
        [JsonProperty("num_reports")]
        public int? ReportCount { get; private set; }

        /// <summary>
        /// Returns the distinguish type.
        /// </summary>
        [JsonProperty("distinguished")]
        [JsonConverter(typeof(DistinguishConverter))]
        public DistinguishType Distinguished { get; private set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not.
        /// Null if they have not cast a vote.
        /// </summary>
        [JsonProperty("likes")]
        public bool? Liked { get; private set; }

        /// <summary>
        /// Returns a list of reports made by moderators.
        /// </summary>
        [JsonProperty("mod_reports")]
        [JsonConverter(typeof(ReportCollectionConverter))]
        public ICollection<Report> ModReports { get; private set; }

        /// <summary>
        /// Returns a list of reports made by users.
        /// </summary>
        [JsonProperty("user_reports")]
        [JsonConverter(typeof(ReportCollectionConverter))]
        public ICollection<Report> UserReports { get; private set; }

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

        /// <summary>
        /// Reports someone
        /// </summary>
        /// <param name="reportType">What you're reporting them for <see cref="ReportType"/></param>
        /// <param name="otherReason">If your reason is "Other", say why you're reporting them</param>
        public async Task ReportAsync(ReportType reportType, string otherReason = null)
        {

            string reportReason;
            switch (reportType)
            {
                case ReportType.Spam:
                    reportReason = "spam"; break;
                case ReportType.VoteManipulation:
                    reportReason = "vote manipulation"; break;
                case ReportType.PersonalInformation:
                    reportReason = "personal information"; break;
                case ReportType.BreakingReddit:
                    reportReason = "breaking reddit"; break;
                case ReportType.SexualizingMinors:
                    reportReason = "sexualizing minors"; break;
                default:
                    reportReason = "other"; break;
            }

            await WebAgent.Post(ReportUrl, new
            {
                api_type = "json",
                reason = reportReason,
                other_reason = otherReason ?? "",
                thing_id = FullName
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Distinguish an item
        /// </summary>
        /// <param name="distinguishType">Type you want to distinguish <see cref="DistinguishType"/></param>
        public async Task DistinguishAsync(DistinguishType distinguishType)
        {

            string how;
            switch (distinguishType)
            {
                case DistinguishType.Admin:
                    how = "admin";
                    break;
                case DistinguishType.Moderator:
                    how = "yes";
                    break;
                case DistinguishType.None:
                    how = "no";
                    break;
                default:
                    how = "special";
                    break;
            }
            var json = await WebAgent.Post(DistinguishUrl, new
            {
                how,
                id = Id
            }).ConfigureAwait(false);
            if (json["jquery"].Count(i => i[0].Value<int>() == 11 && i[1].Value<int>() == 12) == 0)
                throw new Exception("You are not permitted to distinguish this comment.");
        }

        /// <summary>
        /// Approve this item.
        /// </summary>
        /// <returns></returns>
        public Task ApproveAsync()
        {
            return SimpleActionAsync(ApproveUrl);
        }

        /// <summary>
        /// Remove this item.
        /// </summary>
        /// <returns></returns>
        public Task RemoveAsync()
        {
            return RemoveImplAsync(false);
        }

        /// <summary>
        /// Remove this item and flag it as spam.
        /// </summary>
        /// <returns></returns>
        public Task RemoveSpamAsync()
        {
            return RemoveImplAsync(true);
        }

        #pragma warning disable 1591
        protected async Task RemoveImplAsync(bool spam)
        {
            await WebAgent.Post(RemoveUrl, new
            {
                id = FullName,
                spam = spam
            }).ConfigureAwait(false);
        }
        #pragma warning restore 1591

        /// <summary>
        /// Delete this item.
        /// </summary>
        /// <returns></returns>
        public Task DelAsync()
        {
            return SimpleActionAsync(DelUrl);
        }

        /// <summary>
        /// Ignore reports on this item.
        /// </summary>
        /// <returns></returns>
        public Task IgnoreReportsAsync()
        {
            return SimpleActionAsync(IgnoreReportsUrl);
        }

        /// <summary>
        /// Stop ignoring reports on this item.
        /// </summary>
        /// <returns></returns>
        public Task UnIgnoreReportsAsync()
        {
            return SimpleActionAsync(UnIgnoreReportsUrl);
        }

        internal class DistinguishConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(DistinguishType) || objectType == typeof(string);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                var value = token.Value<string>();
                if (value == null)
                    return DistinguishType.None;
                switch (value)
                {
                    case "moderator": return DistinguishType.Moderator;
                    case "admin": return DistinguishType.Admin;
                    case "special": return DistinguishType.Special;
                    default: return DistinguishType.None;
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var d = (DistinguishType)value;
                if (d == DistinguishType.None)
                {
                    writer.WriteNull();
                    return;
                }
                writer.WriteValue(d.ToString().ToLower());
            }
        }

        internal class ReportCollectionConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ICollection<Report>) || objectType == typeof(object);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                if (token.Type != JTokenType.Array || token.Children().Count() == 0)
                    return new Collection<Report>();

                var result = new Collection<Report>();
                foreach (var child in token.Children())
                {
                    // always tuples
                    // https://github.com/reddit/reddit/blob/master/r2/r2/models/report.py#L165
                    if (child.Type != JTokenType.Array || child.Children().Count() != 2)
                        continue;

                    var report = new Report()
                    {
                        Reason = child.First.Value<string>()
                    };
                    if (child.Last.Type == JTokenType.String)
                    {
                        report.ModeratorName = child.Last.Value<string>();
                        report.Count = 1;
                    }
                    else
                    {
                        report.ModeratorName = "";
                        report.Count = child.Last.Value<int>();
                    }
                    result.Add(report);
                }
                return result;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var reports = value as ICollection<Report>;

                if (reports == null || reports.Count == 0)
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    return;
                }

                writer.WriteStartArray();

                foreach (var report in reports)
                {
                    writer.WriteStartArray();

                    writer.WriteValue(report.Reason);

                    if (String.IsNullOrEmpty(report.ModeratorName))
                        writer.WriteValue(report.Count);
                    else
                        writer.WriteValue(report.ModeratorName);

                    writer.WriteEndArray();
                }

                writer.WriteEndArray();
            }
        }
    }

    /// <summary>
    /// A user or moderator report on a <see cref="VotableThing"/>
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Report reason
        /// </summary>
        public string Reason { get; internal set; }

        /// <summary>
        /// Moderator who made the report.  Empty if report was made by
        /// a regular user.
        /// </summary>
        public string ModeratorName { get; internal set; }

        /// <summary>
        /// Number of reports matching <see cref="Reason"/>
        /// </summary>
        public int Count { get; internal set; }
    }
}
