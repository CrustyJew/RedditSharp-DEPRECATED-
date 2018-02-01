using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    /// <summary>
    /// A thing that can be actioned on by a moderator.
    /// </summary>
    public class ModeratableThing : CreatedThing
    {
        /// <summary>
        /// A moderateable thing that can have actions taken against it
        /// </summary>
        /// <param name="agent">An <see cref="IWebAgent"/>to make requests with</param>
        /// <param name="json">A JSON object to init the <see cref="CreatedThing"/> with</param>
        public ModeratableThing(IWebAgent agent, JToken json) : base(agent, json) { }

        /// <summary>
        /// Type of report
        /// </summary>
        public enum ReportType
        {
            Spam = 0,
            VoteManipulation = 1,
            PersonalInformation = 2,
            SexualizingMinors = 3,
            BreakingReddit = 4,
            Other = 5
        }

        /// <summary>
        /// Type of distinguish used for this <see cref="ModeratableThing"/>.
        /// </summary>
        public enum DistinguishType
        {
            /// <summary>
            /// No manual distinguish applied. Other (submitter and friend) distinguishes 
            /// may be applied.
            /// </summary>
            None,
            /// <summary>
            /// Distinguished with a green [M], denoting an official statement from a moderator
            /// </summary>
            Moderator,
            /// <summary>
            /// Distinguished with a red [A], denoting an official statement from an admin
            /// </summary>
            Admin,
            /// <summary>
            /// Various other distinguishes that certain users are able to apply themselves
            /// </summary>
            Special,
        }

        private const string ReportUrl = "/api/report";
        private const string DistinguishUrl = "/api/distinguish";

        private const string ApproveUrl = "/api/approve";
        private const string DelUrl = "/api/del";
        private const string RemoveUrl = "/api/remove";
        private const string IgnoreReportsUrl = "/api/ignore_reports";
        private const string UnIgnoreReportsUrl = "/api/unignore_reports";

        /// <summary>
        /// The moderator who approved this item. This will be null or empty if the item has not been approved.
        /// </summary>
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; private set; }

        /// <summary>
        /// Author user name.
        /// </summary>
        [JsonProperty("author")]
        public string AuthorName { get; private set; }

        /// <summary>
        /// The moderator who removed this item. This will be null or empty if the item has not been removed.<br/>
        /// If this is removed by an administrator (and the user is not in admin mode) or the spam filter, the
        /// value will be 'true'.
        /// </summary>
        [JsonProperty("banned_by")]
        public string BannedBy { get; private set; }

        /// <summary>
        /// Current score of this item.
        /// </summary>
        [JsonProperty("score")]
        public int Score { get; private set; }

        /// <summary>
        /// Returns true if this item has been approved.<br/>
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
        /// Distinguishes (or undistinguishes) an item
        /// </summary>
        /// <param name="distinguishType">Type you want to distinguish <see cref="DistinguishType"/></param>
        /// <param name="sticky">Stickies the Thing if applicable</param>
        public async Task DistinguishAsync(DistinguishType distinguishType, bool sticky = false)
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
                id = FullName, //oAuth requires the full ID
                sticky = sticky
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
        /// <summary>
        /// An implied remove type, doesn't need to be called
        /// </summary>
        /// <param name="spam">Whether or not to mark removed item as spam</param>
        /// <returns></returns>
        protected async Task RemoveImplAsync(bool spam)
        {
            await WebAgent.Post(RemoveUrl, new
            {
                id = FullName,
                spam = spam
            }).ConfigureAwait(false);
        }

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



        #region Static Operations
        /// <summary>
        /// Removes the <see cref="Thing"/>
        /// </summary>
        /// <param name="agent"><see cref="IWebAgent"/> used to send post</param>
        /// <param name="fullname">FullName of thing to act on. eg. t1_66666</param>
        /// <returns></returns>
        public static Task RemoveAsync(IWebAgent agent, string fullname)
        {
            return agent.Post(RemoveUrl, new
            {
                id = fullname,
                spam = false
            });
        }

        /// <summary>
        /// Spams the <see cref="Thing"/>
        /// </summary>
        /// <param name="agent"><see cref="IWebAgent"/> used to send post</param>
        /// <param name="fullname">FullName of thing to act on. eg. t1_66666</param>
        /// <returns></returns>
        public static Task SpamAsync(IWebAgent agent, string fullname)
        {
            return agent.Post(RemoveUrl, new
            {
                id = fullname,
                spam = true
            });
        }
        /// <summary>
        /// Approves the thing <paramref name="fullname"/>
        /// </summary>
        /// <param name="agent"><see cref="IWebAgent"/> used to send post</param>
        /// <param name="fullname">FullName of thing to act on. eg. t1_66666</param>
        /// <returns></returns>
        public static Task ApproveAsync(IWebAgent agent, string fullname)
        {
            return Thing.SimpleActionAsync(agent, fullname, ApproveUrl);
        }

        /// <summary>
        /// Reports someone
        /// </summary>
        /// <param name="reportType">What you're reporting them for <see cref="ReportType"/></param>
        /// <param name="otherReason">If your reason is "Other", say why you're reporting them</param>
        /// <param name="agent"><see cref="IWebAgent"/> used to send post</param>
        /// <param name="fullname">FullName of thing to act on. eg. t1_66666</param>
        public static Task ReportAsync(IWebAgent agent, string fullname, ReportType reportType, string otherReason = null)
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

            return agent.Post(ReportUrl, new
            {
                api_type = "json",
                reason = reportReason,
                other_reason = otherReason ?? "",
                thing_id = fullname
            });
        }
        #endregion

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
                        report.ModeratorName = String.Empty;
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
    /// A user or moderator report on a <see cref="ModeratableThing"/>
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
