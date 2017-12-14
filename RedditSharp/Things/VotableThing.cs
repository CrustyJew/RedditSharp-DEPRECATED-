using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class VotableThing : CreatedThing
    {
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
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="webAgent"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected async Task<VotableThing> InitAsync(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            await CommonInitAsync(reddit, webAgent, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, Reddit.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="webAgent"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected VotableThing Init(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            CommonInit(reddit, webAgent, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, Reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            Init(reddit, json);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        private async Task CommonInitAsync(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            await InitAsync(reddit, json);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        protected virtual void RemoveImpl(bool spam)
        {
            var request = WebAgent.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                spam = spam,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// The moderator who approved this item.  This will be null or empty if the item has not been approved.
        /// </summary>
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Author user name.
        /// </summary>
        [JsonProperty("author")]
        public string AuthorName { get; set; }

        /// <summary>
        /// Css flair class of the item author.
        /// </summary>
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        /// <summary>
        /// Flair text of the item author.
        /// </summary>
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        /// <summary>
        /// The moderator who removed this item.  This will be null or empty if the item has not been removed.
        /// </summary>
        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }

        /// <summary>
        /// Number of upvotes on this item.
        /// </summary>
        [JsonProperty("downs")]
        public int Downvotes { get; set; }

        /// <summary>
        /// Returns true if this item has been edited by the author.
        /// </summary>
        [JsonProperty("edited")]
        public bool Edited { get; set; }

        /// <summary>
        /// Returns true if this item is archived.
        /// </summary>
        [JsonProperty("archived")]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Returns true if this item has been approved.
        /// Returns false if the item has not been approved.  A value of false does not indicate
        /// an item has been removed.
        /// 
        /// <para>Returns null if the logged in user is not a moderator in the items subreddit.</para>
        /// </summary>
        [JsonProperty("approved")]
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Returns true if this item has been removed.
        /// Returns false if the item has not been removed.  A value of false does not indicate
        /// an item has been approved.
        /// 
        /// <para>Returns null if the logged in user is not a moderator in the items subreddit.</para>
        /// </summary>
        [JsonProperty("removed")]
        public bool? IsRemoved { get; set; }

        /// <summary>
        /// Number of upvotes on this item.
        /// </summary>
        [JsonProperty("ups")]
        public int Upvotes { get; set; }

        /// <summary>
        /// Current score of this item.
        /// </summary>
        [JsonProperty("score")]
        public int Score { get; set; }

        /// <summary>
        /// Returns true if this item is saved.
        /// </summary>
        [JsonProperty("saved")]
        public bool Saved { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        /// <summary>
        /// Shortlink to the item
        /// </summary>
        public virtual string Shortlink
        {
            get { return "http://redd.it/" + Id; }
        }

        /// <summary>
        /// Returns true if the item is sticked.
        /// </summary>
        [JsonProperty("stickied")]
        public bool IsStickied { get; set; }

        /// <summary>
        /// Number of reports on this item.
        /// </summary>
        [JsonIgnore]
        [Obsolete("Use ReportCount instead.", false)]
        public int? NumReports => ReportCount;

        /// <summary>
        /// Number of reports on this item.
        /// </summary>
        [JsonProperty("num_reports")]
        public int? ReportCount { get; set; }

        /// <summary>
        /// Returns the distinguish type.
        /// </summary>
        [JsonProperty("distinguished")]
        [JsonConverter(typeof(DistinguishConverter))]
        public DistinguishType Distinguished { get; set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not.
        /// Null if they have not cast a vote.
        /// </summary>
        [JsonProperty("likes")]
        public bool? Liked { get; set; }

        /// <summary>
        /// Returns a list of reports made by moderators.
        /// </summary>
        [JsonProperty("mod_reports")]
        [JsonConverter(typeof(ReportCollectionConverter))]
        public ICollection<Report> ModReports { get; set; }

        /// <summary>
        /// Returns a list of reports made by users.
        /// </summary>
        [JsonProperty("user_reports")]
        [JsonConverter(typeof(ReportCollectionConverter))]
        public ICollection<Report> UserReports { get; set; }

        /// <summary>
        /// Number of times this item has been gilded.
        /// </summary>
        [JsonProperty("gilded")]
        public int Gilded { get; set; }

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
            set { this.SetVote(value); }
        }
        /// <summary>
        /// Upvotes something
        /// </summary>
        public void Upvote()
        {
            this.SetVote(VoteType.Upvote);
        }

        /// <summary>
        /// Downvote this item.
        /// </summary>
        public void Downvote()
        {
            this.SetVote(VoteType.Downvote);
        }

        /// <summary>
        /// Vote on this item.
        /// </summary>
        /// <param name="type"></param>
        public void SetVote(VoteType type)
        {
            if (this.Vote == type) return;

            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = (int)type,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());

            if (Liked == true) Upvotes--;
            if (Liked == false) Downvotes--;

            switch(type)
            {
                case VoteType.Upvote: Liked = true; Upvotes++; return;
                case VoteType.None: Liked = null; return;
                case VoteType.Downvote: Liked = false; Downvotes++; return;
            }
        }

        /// <summary>
        /// Save this item.
        /// </summary>
        public void Save()
        {
            var request = WebAgent.CreatePost(SaveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Saved = true;
        }

        /// <summary>
        /// Unsave this item.
        /// </summary>
        public void Unsave()
        {
            var request = WebAgent.CreatePost(UnsaveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Saved = false;
        }

        /// <summary>
        /// Clear you vote on this item.
        /// </summary>
        public void ClearVote()
        {
            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = 0,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Reports someone
        /// </summary>
        /// <param name="reportType">What you're reporting them for <see cref="ReportType"/></param>
        /// <param name="otherReason">If your reason is "Other", say why you're reporting them</param>
        public void Report(ReportType reportType, string otherReason = null)
        {
            var request = WebAgent.CreatePost(ReportUrl);
            var stream = request.GetRequestStream();

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

            WebAgent.WritePostBody(stream, new
            {
                api_type = "json",
                reason = reportReason,
                other_reason = otherReason ?? "",
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Distinguish an item
        /// </summary>
        /// <param name="distinguishType">Type you want to distinguish <see cref="DistinguishType"/></param>
        public void Distinguish(DistinguishType distinguishType)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(DistinguishUrl);
            var stream = request.GetRequestStream();
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
            WebAgent.WritePostBody(stream, new
            {
                how,
                id = Id,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["jquery"].Count(i => i[0].Value<int>() == 11 && i[1].Value<int>() == 12) == 0)
                throw new AuthenticationException("You are not permitted to distinguish this item.");
        }

        /// <summary>
        /// Approve this item.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        public void Approve()
        {
            var data = SimpleAction(ApproveUrl);
        }

        /// <summary>
        /// Remove this item.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        public void Remove()
        {
            RemoveImpl(false);
        }

        /// <summary>
        /// Remove this item, flagging it as spam.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        public void RemoveSpam()
        {
            RemoveImpl(true);
        }

        /// <summary>
        /// Delete this item.  Logged in user must be the items author.
        /// </summary>
        public void Del()
        {
            var data = SimpleAction(DelUrl);
        }

        /// <summary>
        /// Ignore reports on this item.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        public void IgnoreReports()
        {
            var data = SimpleAction(IgnoreReportsUrl);
        }

        /// <summary>
        /// Unignore reports on this item.  Logged in user must be a moderator of parent subreddit.
        /// </summary>
        public void UnIgnoreReports()
        {
            var data = SimpleAction(UnIgnoreReportsUrl);
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

    public class Report
    {
        /// <summary>
        /// Report reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Moderator who made the report.  Empty if report was made by
        /// a regular user.
        /// </summary>
        public string ModeratorName { get; set; }

        /// <summary>
        /// Number of reports matching <see cref="Reason"/>
        /// </summary>
        public int Count { get; set; }
    }
}
