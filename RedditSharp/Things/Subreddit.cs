using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions.DateTimeExtensions;

namespace RedditSharp.Things
{
    /// <summary>
    /// Represents a subreddit.
    /// </summary>
    public class Subreddit : Thing
    {
        /// <summary>
        /// Sorting for posts in a a subreddit.
        /// </summary>
        public enum Sort
        {
            /// <summary>
            /// Sort by newest first
            /// </summary>
            New,
            /// <summary>
            /// Sort by rising threads
            /// </summary>
            Rising,
            /// <summary>
            /// Sort by hot threads
            /// </summary>
            Hot,
            /// <summary>
            /// Sort by top threads
            /// </summary>
            Top,
            /// <summary>
            /// Sort by controversial
            /// </summary>
            Controversial
        }

        #region Properties
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private string SubredditPostUrl => $"/r/{Name}.json";
        private string SubredditNewUrl => $"/r/{Name}/new.json?sort=new";
        private string SubredditHotUrl => $"/r/{Name}/hot.json"; // this is the default sort?
        private string SubredditRisingUrl => $"/r/{Name}/rising.json";
        private string SubredditTopUrl => $"/r/{Name}/top.json";
        private string SubredditControversialUrl => $"/r/{Name}/controversial.json";
        private string SubredditGildedUrl => $"/r/{Name}/gilded.json";
        private const string SubscribeUrl = "/api/subscribe";
        private string GetSettingsUrl => $"/r/{Name}/about/edit.json";
        private string GetReducedSettingsUrl => $"/r/{Name}/about.json";
        private string ModqueueUrl => $"/r/{Name}/about/modqueue.json";
        private string UnmoderatedUrl => $"/r/{Name}/about/unmoderated.json";
        private string FlairTemplateUrl => $"/r/{Name}/api/flairtemplate";
        private string ClearFlairTemplatesUrl => $"/r/{Name}/api/clearflairtemplates";
        private string SetUserFlairUrl => $"/r/{Name}/api/flair";
        private string StylesheetUrl => $"/r/{Name}/about/stylesheet.json";
        private const string UploadImageUrl = "/api/upload_sr_img";
        private string FlairSelectorUrl => $"/r/{Name}/api/flairselector";
        private const string AcceptModeratorInviteUrl = "/api/accept_moderator_invite";
        private const string LeaveModerationUrl = "/api/unfriend";
        private const string BanUserUrl = "/api/friend";
        private const string UnBanUserUrl = "/api/unfriend";
        private const string AddModeratorUrl = "/api/friend";
        private const string AddContributorUrl = "/api/friend";
        private string ModeratorsUrl => $"/r/{Name}/about/moderators.json";
        private const string FrontPageUrl = "/.json";
        private const string SubmitLinkUrl = "/api/submit";
        private string FlairListUrl => $"/r/{Name}/api/flairlist.json";
        private string CommentsUrl => $"/r/{Name}/comments.json";
        private string SearchUrl(string query, string sort, string time) =>
          $"/r/{Name}/search.json?q={query}&restrict_sr=on&sort={sort}&t={time}";
        private string SearchUrlDate(double from, double to, string sort) =>
            $"/r/{Name}/search.json?q=timestamp:{from}..{to}&restrict_sr=on&sort={sort}&syntax=cloudsearch";
        private string ModLogUrl => $"/r/{Name}/about/log.json";
        private string ContributorsUrl => $"/r/{Name}/about/contributors.json";
        private string BannedUsersUrl => $"/r/{Name}/about/banned.json";
        private string ModmailUrl => $"/r/{Name}/message/moderator/inbox.json";

        /// <summary>
        /// Get the subreddit Wiki
        /// </summary>
        [JsonIgnore]
        public Wiki GetWiki => new Wiki(WebAgent, this.Name);

        /// <summary>
        /// Date the subreddit was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; private set; }

        /// <summary>
        /// Subreddit description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Subreddit description html.
        /// </summary>
        [JsonProperty("description_html")]
        public string DescriptionHTML { get; private set; }

        /// <summary>
        /// Subreddit display name.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Header image.
        /// </summary>
        [JsonProperty("header_img")]
        public string HeaderImage { get; private set; }

        /// <summary>
        /// Header title.
        /// </summary>
        [JsonProperty("header_title")]
        public string HeaderTitle { get; private set; }

        /// <summary>
        /// Returns true of the subreddit is marked for users over 18.
        /// </summary>
        [JsonProperty("over_18")]
        public bool NSFW { get; private set; }

        /// <summary>
        /// Public description of the subreddit.
        /// </summary>
        [JsonProperty("public_description")]
        public string PublicDescription { get; private set; }

        /// <summary>
        /// Total subscribers to the subreddit.
        /// </summary>
        [JsonProperty("subscribers")]
        public int? Subscribers { get; private set; }

        /// <summary>
        /// Current active users .
        /// </summary>
        [JsonProperty("accounts_active")]
        public int? ActiveUsers { get; private set; }

        /// <summary>
        /// Subreddit title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }


        /// <summary>
        /// Prefix for fullname. Includes trailing underscore
        /// </summary>
        public static string KindPrefix { get { return "t5_"; } }

        /// <summary>
        /// Subreddit url.
        /// </summary>
        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; private set; }

        /// <summary>
        /// Property determining whether the current logged in user is a moderator on this subreddit.
        /// </summary>
        [JsonProperty("user_is_moderator")]
        public bool? UserIsModerator { get; private set; }

        /// <summary>
        /// Property giving the moderator permissions of the logged in user on this subreddit.
        /// </summary>
        [JsonProperty("mod_permissions")]
        [JsonConverter(typeof(ModeratorPermissionConverter))]
        public ModeratorPermission ModPermissions { get; private set; }

        /// <summary>
        /// Property determining whether the current logged in user is banned from the subreddit.
        /// </summary>
        [JsonProperty("user_is_banned")]
        public bool? UserIsBanned { get; private set; }

        /// <summary>
        /// Name of the subreddit.
        /// </summary>
        [JsonIgnore]
        public string Name { get; private set; }

        #endregion

        /// <summary>
        /// Create and populate Subreddit info from <paramref name="json"/>
        /// </summary>
        /// <param name="agent">IWebAgent to use</param>
        /// <param name="json">JSON data with subreddit info</param>
        public Subreddit(IWebAgent agent, JToken json) : base(agent, json)
        {
            SetName();
        }
        /// <summary>
        /// Returns a <see cref="Subreddit"/> by name, retrieved using given WebAgent
        /// </summary>
        /// <param name="agent">IWebAgent to use</param>
        /// <param name="name">Name of subreddit. Will remove 'r/' if it is included.</param>
        /// <returns></returns>
        public static Task<Subreddit> GetByNameAsync(IWebAgent agent, string name)
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(r/|/)", "");
            return Helpers.GetThingAsync<Subreddit>(agent, string.Format(SubredditAboutUrl, name));
        }

        /// <summary>
        /// Top of the subreddit.
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        /// <returns>The top of the subreddit from a specific time</returns>
        public Listing<Post> GetTop(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/top.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditTopUrl, max, 100);
        }

        /// <summary>
        /// Top of the subreddit from a particular time.
        /// </summary>
        /// <param name="timePeriod">Timeperiod you want to start at <seealso cref="FromTime"/></param>
        /// <param name="max">Maximum number of records to return.</param>
        /// <returns>The top of the subreddit from a specific time</returns>
        public Listing<Post> GetTop(FromTime timePeriod, int max = -1)
        {
            var period = timePeriod.ToString("g").ToLower();
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/top.json?t=" + period, max, 100);

            return Listing<Post>.Create(WebAgent, $"{SubredditTopUrl}?t={period}", max, 100);
        }

        /// <summary>
        /// Comments for a subreddit, all of them, irrespective of replies and what it is replying to
        /// </summary>
        public Listing<Comment> GetComments(int max = -1)
        {
            if (Name == "/")
                return Listing<Comment>.Create(WebAgent, "/comments.json", max, 100);

            return Listing<Comment>.Create(WebAgent, CommentsUrl, max, 100);
        }

        /// <summary>
        /// Posts on the subreddit/new
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        private Listing<Post> GetNew(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/new.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditNewUrl, max, 100);
        }

        /// <summary>
        /// Posts on the front page of the subreddits
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        private Listing<Post> GetHot(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditHotUrl, max, 100);
        }

        /// <summary>
        /// List of rising posts
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        private Listing<Post> GetRising(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditRisingUrl, max, 100);
        }

        /// <summary>
        /// List of Controversial posts
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        private Listing<Post> GetControversial(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditControversialUrl, max, 100);
        }

        /// <summary>
        /// All posts on a subreddit.
        /// </summary>
        /// /// <param name="max">Maximum number of records to return. -1 for unlimited.</param>
        public Listing<Post> GetPosts(int max = -1)
        {
            if (Name == "/")
                return Listing<Post>.Create(WebAgent, "/.json", max, 100);

            return Listing<Post>.Create(WebAgent, SubredditPostUrl, max, 100);
        }

        /// <summary>
        /// Get posts on a subreddit.
        /// </summary>
        /// <param name="sort">How to sort the results.</param>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Post> GetPosts(Sort sort, int max = -1)
        {
            switch (sort)
            {
                case Sort.New:
                    return GetNew(max);
                case Sort.Rising:
                    return GetRising(max);
                case Sort.Hot:
                    return GetHot(max);
                case Sort.Top:
                    return GetTop(max);
                case Sort.Controversial:
                    return GetControversial(max);
            }
            return null;
        }


        /// <summary>
        /// List of gilded things
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<VotableThing> GetGilded(int max = -1)
        {
            if (Name == "/")
                return Listing<VotableThing>.Create(WebAgent, "/.json", max, 100);

            return Listing<VotableThing>.Create(WebAgent, SubredditGildedUrl, max, 100);
        }

        /// <summary>
        /// List of items in the mod queue
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<VotableThing> GetModQueue(int max = -1) => Listing<VotableThing>.Create(WebAgent, ModqueueUrl, max, 100);

        /// <summary>
        /// Links a moderator hasn't checked
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Post> GetUnmoderatedLinks(int max = -1) => Listing<Post>.Create(WebAgent, UnmoderatedUrl, max, 100);

        /// <summary>
        /// Search using specific terms from a specified time to now
        /// </summary>
        /// <param name="terms">Terms you want to search for</param>
        /// <param name="sortE">Sort the way you want to, see <see cref="Sorting"/></param>
        /// <param name="timeE">Time sorting you want to see</param>
        /// <param name="max">Number of records to return.  -1 for unliminted.</param>
        /// <returns>A list of posts</returns>
        public Listing<Post> Search(string terms, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All, int max = -1)
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            return Listing<Post>.Create(WebAgent, SearchUrl(Uri.EscapeUriString(terms), sort, time), max, 100);
        }

        /// <summary>
        /// Search for a list of posts from a specific time to another time
        /// </summary>
        /// <param name="from">Time to begin search</param>
        /// <param name="to">Time to end search at</param>
        /// <param name="sortE">Sort of the objects you want to have it in</param>
        /// <param name="max">Number of records to return.  -1 for unliminted.</param>
        /// <returns>A list of posts in the range of time/dates in a specific order</returns>
        public Listing<Post> Search(DateTime from, DateTime to, Sorting sortE = Sorting.New, int max = -1)
        {
            string sort = sortE.ToString().ToLower();

            return Listing<Post>.Create(WebAgent,
                SearchUrlDate(from.DateTimeToUnixTimestamp(),
                  to.DateTimeToUnixTimestamp(), sort), max, 100);
        }

        /// <summary>
        /// Settings of the subreddit, as best as possible
        /// </summary>
        public async Task<SubredditSettings> GetSettingsAsync()
        {
            try
            {
                var json = await WebAgent.Get(GetSettingsUrl).ConfigureAwait(false);
                return new SubredditSettings(this, json);
            }
            catch // TODO: More specific catch
            {
                // Do it unauthed
                var json = await WebAgent.Get(GetReducedSettingsUrl).ConfigureAwait(false);
                return new SubredditSettings(this, json);
            }
        }

        /// <summary>
        /// Get an array of the available user flair templates for the subreddit
        /// </summary>
        public async Task<IEnumerable<UserFlairTemplate>> GetUserFlairTemplatesAsync()
        {
            //TODO Unit Tests
            var json = await WebAgent.Post(FlairSelectorUrl, new { }).ConfigureAwait(false);
            var choices = json["choices"];
            var list = new List<UserFlairTemplate>();
            foreach (var choice in choices)
            {
                UserFlairTemplate template = JsonConvert.DeserializeObject<UserFlairTemplate>(choice.ToString());
                list.Add(template);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get the subreddit stylesheet.
        /// </summary>
        public async Task<SubredditStyle> GetStylesheetAsync()
        {
            var json = await WebAgent.Get(StylesheetUrl).ConfigureAwait(false);
            return new SubredditStyle(this, json);
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the subreddit moderators.
        /// </summary>
        public async Task<IEnumerable<ModeratorUser>> GetModeratorsAsync()
        {
            var json = await WebAgent.Get(ModeratorsUrl).ConfigureAwait(false);
            var type = json["kind"].ToString();
            if (type != "UserList")
                throw new FormatException("Reddit responded with an object that is not a user listing.");
            var data = json["data"];
            var mods = data["children"].ToArray();
            var result = new ModeratorUser[mods.Length];
            for (var i = 0; i < mods.Length; i++)
            {
                var mod = new ModeratorUser(mods[i]);
                result[i] = mod;
            }
            return result;
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of toolbox user notes.
        /// </summary>
        public Task<IEnumerable<TBUserNote>> GetUserNotesAsync() =>
            ToolBoxUserNotes.GetUserNotesAsync(WebAgent, Name);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of contributors.
        /// </summary>
        public Listing<Contributor> GetContributors(int max = -1) => Listing<Contributor>.Create(WebAgent, ContributorsUrl, max, 100);

        /// <summary>
        /// Get a <see cref="Listing{T}"/> of banned users.
        /// </summary>
        public Listing<BannedUser> GetBannedUsers(int max = -1) => Listing<BannedUser>.Create(WebAgent, BannedUsersUrl, max, 100);

        /// <summary>
        /// Subreddit modmail.
        /// <para/>
        ///  When calling <see cref="System.Linq.Enumerable.Take{T}"/> make sure to take replies into account!
        /// </summary>
        public Listing<PrivateMessage> GetModmail()
        {
            return new Listing<PrivateMessage>(WebAgent, ModmailUrl);
        }

        /// <inheritdoc />
        internal override JToken GetJsonData(JToken json) => json["data"];

        private void SetName()
        {
            Name = Url.ToString();
            if (Name.StartsWith("/r/"))
                Name = Name.Substring(3);
            if (Name.StartsWith("r/"))
                Name = Name.Substring(2);
            Name = Name.TrimEnd('/');
        }

        /// <summary>
        /// http://www.reddit.com/r/all
        /// </summary>
        /// <param name="agent">If logged in, helps personilization</param>
        /// <returns>http://www.reddit.com/r/all</returns>
        public static Subreddit GetRSlashAll(IWebAgent agent)
        {
            var rSlashAll = new Subreddit(agent, null)
            {
                DisplayName = "/r/all",
                Title = "/r/all",
                Url = new Uri("/r/all", UriKind.Relative),
                Name = "all",
            };
            return rSlashAll;
        }

        /// <summary>
        /// Gets the frontpage of the user
        /// </summary>
        /// <param name="agent">WebAgent with logged in user</param>
        /// <returns>the frontpage of reddit</returns>
        public static Subreddit GetFrontPage(IWebAgent agent)
        {
            var frontPage = new Subreddit(agent, null)
            {
                DisplayName = "Front Page",
                Title = "reddit: the front page of the internet",
                Url = new Uri("/", UriKind.Relative),
                Name = "/",
            };
            return frontPage;
        }

        /// <summary>
        /// Subscribe to a subreddit
        /// </summary>
        public async Task SubscribeAsync()
        {
            await WebAgent.Post(SubscribeUrl, new
            {
                action = "sub",
                sr = FullName
            });
            //Disposes and discards
        }

        /// <summary>
        /// Unsubscribes from a subreddit
        /// </summary>
        public async Task UnsubscribeAsync()
        {
            await WebAgent.Post(SubscribeUrl, new
            {
                action = "unsub",
                sr = FullName
            }).ConfigureAwait(false);
            //Dispose and discard
        }

        /// <summary>
        /// Clear templates of specified <see cref="FlairType"/>
        /// </summary>
        /// <param name="flairType"><see cref="FlairType"/></param>
        public async Task ClearFlairTemplatesAsync(FlairType flairType)
        {
            //TODO Unit Tests
            await WebAgent.Post(ClearFlairTemplatesUrl, new
            {
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR"
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a new flair template.
        /// </summary>
        /// <param name="cssClass">css class name</param>
        /// <param name="flairType"><see cref="FlairType"/></param>
        /// <param name="text">flair text</param>
        /// <param name="userEditable">set flair user editable</param>
        public async Task AddFlairTemplateAsync(string cssClass, FlairType flairType, string text, bool userEditable)
        {
            //TODO Unit Tests
            await WebAgent.Post(FlairTemplateUrl, new
            {
                css_class = cssClass,
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                text = text,
                text_editable = userEditable,
                api_type = "json"
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the flair text of a user.
        /// </summary>
        /// <param name="max">Maximum number of records to return.</param>
        public Listing<RedditUser> GetUserFlairList(int max = -1) => Listing<RedditUser>.Create(WebAgent, FlairListUrl, max, 1000);

        /// <summary>
        /// Get the flair text of a user.
        /// </summary>
        /// <param name="user">user name.</param>
        public async Task<string> GetFlairTextAsync(string user)
        {
            var json = await WebAgent.Get(FlairListUrl + "?name=" + user).ConfigureAwait(false);
            return (string)json["users"][0]["flair_text"];
        }

        /// <summary>
        /// Get the css class of the specified users flair.
        /// </summary>
        /// <param name="user">reddit username</param>
        /// <returns></returns>
        public async Task<string> GetFlairCssClassAsync(string user)
        {
            var json = await WebAgent.Get(FlairListUrl + "?name=" + user).ConfigureAwait(false);
            return (string)json["users"][0]["flair_css_class"];
        }

        /// <summary>
        /// Set a users flair.
        /// </summary>
        /// <param name="user">reddit username</param>
        /// <param name="cssClass">flair css class</param>
        /// <param name="text">flair text</param>
        public async Task SetUserFlairAsync(string user, string cssClass, string text)
        {
            //TODO Unit Tests
            await WebAgent.Post(SetUserFlairUrl, new
            {
                css_class = cssClass,
                text = text,
                name = user
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload a header image.
        /// </summary>
        /// <param name="name">name of image.</param>
        /// <param name="imageType"><see cref="ImageType"/> of image</param>
        /// <param name="file">image buffer</param>
        public async Task UploadHeaderImageAsync(string name, ImageType imageType, byte[] file)
        {
            var request = WebAgent.CreateRequest(UploadImageUrl, "POST");
            var formData = new MultipartFormBuilder(request);
            formData.AddDynamic(new
            {
                name,
                r = Name,
                formid = "image-upload",
                img_type = imageType == ImageType.PNG ? "png" : "jpg",
                upload = "",
                header = 1
            });
            formData.AddFile("file", "foo.png", file, imageType == ImageType.PNG ? "image/png" : "image/jpeg");
            formData.Finish();
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            // TODO: Detect errors
        }

        /// <summary>
        /// Adds a moderator
        /// </summary>
        /// <param name="user">User to add, by username</param>
        public async Task AddModeratorAsync(string user)
        {
            await WebAgent.Post(AddModeratorUrl, new
            {
                api_type = "json",
                r = Name,
                type = "moderator",
                name = user
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Accept invitation to moderate this subreddit.
        /// </summary>
        public async Task AcceptModeratorInviteAsync()
        {
            await WebAgent.Post(AcceptModeratorInviteUrl, new
            {
                api_type = "json",
                r = Name
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a moderator from this subreddit.
        /// </summary>
        /// <param name="id">reddit user fullname</param>
        public async Task RemoveModeratorAsync(string id)
        {
            await WebAgent.Post(LeaveModerationUrl, new
            {
                api_type = "json",
                r = Name,
                type = "moderator",
                id
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override string ToString() => "/r/" + DisplayName;

        /// <summary>
        /// Add a contributor to this subreddit.
        /// </summary>
        /// <param name="user">reddit username.</param>
        public async Task AddContributorAsync(string user)
        {
            await WebAgent.Post(AddContributorUrl, new
            {
                api_type = "json",
                r = Name,
                type = "contributor",
                name = user
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a contributor from this subreddit.
        /// </summary>
        /// <param name="id">reddit user full name</param>
        public async Task RemoveContributorAsync(string id)
        {
            await WebAgent.Post(LeaveModerationUrl, new
            {
                api_type = "json",
                r = Name,
                type = "contributor",
                id
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Bans a user
        /// </summary>
        /// <param name="user">User to ban, by username</param>
        /// <param name="reason">Reason for ban, shows in ban note as 'reason: note' or just 'note' if blank</param>
        /// <param name="note">Mod notes about ban, shows in ban note as 'reason: note'</param>
        /// <param name="duration">Number of days to ban user, 0 for permanent</param>
        /// <param name="message">Message to include in ban PM</param>
        public async Task BanUserAsync(string user, string reason, string note, int duration, string message)
        {
            await WebAgent.Post(BanUserUrl, new
            {
                api_type = "json",
                r = Name,
                container = FullName,
                type = "banned",
                name = user,
                ban_reason = reason,
                note = note,
                duration = duration <= 0 ? "" : duration.ToString(),
                ban_message = message
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Ban a user by name
        /// </summary>
        /// <param name="user">user name</param>
        /// <param name="note">ban note</param>
        public Task BanUserAsync(string user, string note) => BanUserAsync(user, "", note, 0, "");

        /// <summary>
        /// Unbans a user
        /// </summary>
        /// <param name="user">User to unban, by username</param>
        public async Task UnBanUserAsync(string user)
        {
            await WebAgent.Post(UnBanUserUrl, new
            {
                r = Name,
                type = "banned",
                container = FullName,
                executed = "removed",
                name = user,
            }).ConfigureAwait(false);
        }

        private async Task<Post> SubmitAsync(SubmitData data, ICaptchaSolver solver = null)
        {
            var json = await WebAgent.Post(SubmitLinkUrl, data).ConfigureAwait(false);

            if (json["errors"].Any() && json["errors"][0][0].ToString() == "BAD_CAPTCHA")
            {
                if (solver == null) throw new CaptchaFailedException("Captcha required but not ICaptchaSolver provided");

                data.Iden = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(data.Iden));

                // We throw exception due to this method being expected to return a valid Post object, but we cannot
                // if we got a Captcha error.
                if (captchaResponse.Cancel)
                    throw new CaptchaFailedException("Captcha verification failed when submitting " + data.Kind + " post");

                data.Captcha = captchaResponse.Answer;
                return await SubmitAsync(data, solver).ConfigureAwait(false);
            }
            else if (json["errors"].Any() && json["errors"][0][0].ToString() == "ALREADY_SUB")
            {
                throw new DuplicateLinkException($"Post failed when submitting.  The following link has already been submitted: {((LinkData)data).URL}");
            }

            return new Post(WebAgent, json["data"]);
        }
        /// <summary>
        /// Submits a link post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="url">The url of the submission link</param>
        /// <param name="captchaId"></param>
        /// <param name="captchaAnswer"></param>
        /// <param name="resubmit"></param>
        public async Task<Post> SubmitPostAsync(string title, string url, string captchaId = "", string captchaAnswer = "", bool resubmit = false)
        {
            return await SubmitAsync(new LinkData
            {
                Subreddit = Name,
                Title = title,
                URL = url,
                Resubmit = resubmit,
                Iden = captchaId,
                Captcha = captchaAnswer
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Submits a text post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="text">The raw markdown text of the submission</param>
        /// <param name="captchaId"></param>
        /// <param name="captchaAnswer"></param>
        public async Task<Post> SubmitTextPostAsync(string title, string text, string captchaId = "", string captchaAnswer = "")
        {
            return await SubmitAsync(new TextData
            {
                Subreddit = Name,
                Title = title,
                Text = text,
                Iden = captchaId,
                Captcha = captchaAnswer
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the moderation log of the current subreddit
        /// </summary>
        public Listing<ModAction> GetModerationLog(int max = -1) => Listing<ModAction>.Create(WebAgent, ModLogUrl, max, 500);

        /// <summary>
        /// Gets the moderation log of the current subreddit filtered by the action taken
        /// </summary>
        /// <param name="action">ModActionType of action performed</param>
        /// <param name="max">Maximum number of records to return.</param>
        public Listing<ModAction> GetModerationLog(ModActionType action, int max = -1)
        {
            var url = $"{ModLogUrl}?type={action.ToString("g").ToLower()}";
            return Listing<ModAction>.Create(WebAgent, url, max, 500);
        }

        /// <summary>
        /// Gets the moderation log of the current subreddit filtered by moderator(s) who performed the action
        /// </summary>
        /// <param name="mods">String array of mods to filter by</param>
        /// <param name="max">Maximum number of records to return.</param>
        public Listing<ModAction> GetModerationLog(IEnumerable<string> mods, int max = -1)
        {
            var url = $"{ModLogUrl}?mod={string.Join(",", mods)}";
            return Listing<ModAction>.Create(WebAgent, url, max, 500);
        }

        /// <summary>
        /// Gets the moderation log of the current subreddit filtered by the action taken and moderator(s) who performed the action
        /// </summary>
        /// <param name="action">ModActionType of action performed.</param>
        /// <param name="mods">String array of mods to filter by.</param>
        /// <param name="max">Maximum number of records to return.</param>
        /// <returns></returns>
        public Listing<ModAction> GetModerationLog(ModActionType action, IEnumerable<string> mods, int max = -1)
        {
            var url = $"{ModLogUrl}?type={action}";
            return Listing<ModAction>.Create(WebAgent, url, max, 500);
        }
    }
}
