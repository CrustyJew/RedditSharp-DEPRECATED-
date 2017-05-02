using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A Reddit Live thread.  https://www.reddit.com/dev/api/#section_live
    /// </summary>
    // https://github.com/reddit/reddit-plugin-liveupdate/blob/master/reddit_liveupdate/models.py#L19
    public class LiveUpdateEvent : CreatedThing
    {
#pragma warning disable 1591
        [Flags]
        public enum LiveUpdateEventPermission
        {
            None = 0,
            Update = 1,
            Manage = 2,
            Settings = 4,
            Edit = 8,
            Close = 16,
            All = Update | Manage | Settings | Edit | Close
        }

        public LiveUpdateEvent(IWebAgent agent, JToken json) : base(agent, json) {
            FullName = Name;
            Name = Name.Replace("LiveUpdateEvent_", "");
        }

#pragma warning restore 1591

        /// <summary>
        /// A user participating in this live event.
        /// </summary>
        public class LiveUpdateEventUser
        {
            /// <summary>
            /// Permissions of the user.
            /// </summary>
            [JsonProperty]
            [JsonConverter(typeof(PermissionsConverter))]
            public LiveUpdateEventPermission Permissions { get; private set; }

            /// <summary>
            /// User name.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; private set; }

            /// <summary>
            /// Base36 id of the user.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; private set; }
        }

        private string AcceptContributorInviteUrl => $"/api/live/{Name}/accept_contribtor_invite";
        private string CloseThreadUrl => $"/api/live/{Name}/close_thread";
        private string EditUrl => $"/api/live/{Id}/edit";
        private string InviteContributorUrl => $"/api/live/{Name}/invite_contributor";
        private string LeaveContributorUrl => $"/api/live/{Name}/leave_contributor";
        private string RemoveContributorUrl => $"/api/live/{Name}/rm_contributor";
        private string RevokeContributorInviteUrl => $"/api/live/{Name}/rm_contributor_invite";
        private string SetContributorPermissionUrl => $"/api/live/{Name}/set_contributor_permissions";
        private string UpdateUrl => $"/api/live/{Name}/update";
        private string StrikeUpdateUrl => $"/api/live/{Name}/strike_update";
        private string DeleteUpdateUrl => $"/api/live/{Name}/delete_update";
        private string GetUrl => $"/live/{Name}";
        private string ContributorsUrl => $"/live/{Name}/contributors.json";
        private string DiscussionsUrl => $"/live/{Name}/discussions";
        private string ReportUrl => $"/api/live/{Name}/report";

        /// <summary>
        /// Description of the live event.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Description of the live event in html.
        /// </summary>
        [JsonProperty("description_html")]
        public string DescriptionHtml { get; private set; }

        /// <summary>
        /// Live event title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }

        /// <summary>
        /// Websocket URL for this live event.
        /// </summary>
        [JsonProperty("websocket_uri")]
        public Uri WebsocketUri { get; private set; }

        /// <summary>
        /// Live event state.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; private set; }

        /// <summary>
        /// Returns true if this live event is marked not safe for work.
        /// </summary>
        [JsonProperty("nsfw")]
        public bool NSFW { get; private set; }

        /// <summary>
        /// Number of people viewing this live event.
        /// </summary>
        [JsonProperty("viewer_count")]
        public int? ViewerCount { get; private set; }

        /// <summary>
        /// Returns true if <see cref="ViewerCount"/> is fuzzed.
        /// </summary>
        [JsonProperty("viewer_count_fuzzed")]
        public bool ViewerCountFuzzed { get; private set; }

        /// <summary>
        /// Resources markdown.
        /// </summary>
        [JsonProperty("resources")]
        public string Resources { get; private set; }

        /// <summary>
        /// Name of the live event.
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Accept an invite to be a live thread contributor.
        /// </summary>
        public Task AcceptContributorInviteAsync() =>
            SimpleActionAsync(AcceptContributorInviteUrl);

        /// <summary>
        /// Close the live thread.
        /// </summary>
        public Task CloseAsync() => SimpleActionAsync(CloseThreadUrl);

        /// <summary>
        /// Delete an update
        /// </summary>
        /// <param name="update">Update to strike</param>
        public async Task<bool> DeleteUpdateAsync(LiveUpdate update)
        {
            var request = WebAgent.CreateRequest(DeleteUpdateUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                id = update.FullName
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;
            return false;
        }

        /// <summary>
        /// Edit a live thread.  Set parameters to empty string to clear those fields.  Or null to ignore them on update.
        /// </summary>
        /// <param name="title">New Title.</param>
        /// <param name="description">New Description</param>
        /// <param name="resources">new Resources</param>
        /// <param name="nsfw">NSFW flag</param>
        public async Task<bool> EditAsync(string title, string description, string resources, bool? nsfw)
        {
            var expando = (IDictionary<string, object>)new ExpandoObject();

            if (title != null)
                expando.Add(new KeyValuePair<string, object>("title", title));

            if (description != null)
                expando.Add(new KeyValuePair<string, object>("description", description));

            if (resources != null)
                expando.Add(new KeyValuePair<string, object>("resources", resources));

            if (nsfw.HasValue)
                expando.Add(new KeyValuePair<string, object>("nsfw", nsfw.Value));

            var request = WebAgent.CreateRequest(EditUrl, "POST");
            WebAgent.WritePostBody(request, expando);

            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JToken json = JToken.Parse(data);
            if (json["json"].ToString().Contains("\"errors\": []"))
            {
                Title = title ?? "";
                Description = description ?? "";
                Resources = resources ?? "";

                if (nsfw.HasValue)
                    NSFW = nsfw.Value;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a list of contributors.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LiveUpdateEvent.LiveUpdateEventUser>> GetContributorsAsync()
        {
            var result = new List<LiveUpdateEvent.LiveUpdateEventUser>();
            var json = await WebAgent.Get(ContributorsUrl);

            JToken users;
            if (json.Type == JTokenType.Array)
            {
                users = json[1]["data"]["children"];
            }
            else
            {
                users = json["data"]["children"];
            }
            return users.Select(u => u.ToObject<LiveUpdateEventUser>());
        }

        /// <summary>
        /// Get a list of reddit submissions linking to this thread.
        /// </summary>
        /// <returns></returns>
        public Listing<Post> GetDiscussions() =>
            new Listing<Post>(WebAgent, DiscussionsUrl);

        /// <summary>
        /// Get a list of updates to this live event.
        /// </summary>
        /// <returns></returns>
        public Listing<LiveUpdate> GetThread() =>
            new Listing<LiveUpdate>(WebAgent, GetUrl);

        /// <summary>
        /// Get invited contributors.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LiveUpdateEventUser>> GetInvitedContributorsAsync()
        {
            var json = await WebAgent.Get(ContributorsUrl).ConfigureAwait(false);
            var users = json[1]["data"]["children"];
            return users.Select(u => JsonConvert.DeserializeObject(u.ToString())).Cast<LiveUpdateEventUser>();
        }

        /// <summary>
        /// Invite a contributor to the live thread.
        /// </summary>
        /// <param name="userName">reddit username.</param>
        /// <param name="permissions">permissions.</param>
        public async Task<bool> InviteContributorAsync(string userName, LiveUpdateEventPermission permissions)
        {
            var perms = GetPermissionsString(permissions);
            var request = WebAgent.CreateRequest(InviteContributorUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                name = userName,
                permissions = perms,
                type = "liveupdate_contributor_invite"
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Abdicate contributorship of a thread.
        /// </summary>
        public Task LeaveContributorAsync() => SimpleActionAsync(LeaveContributorUrl);

        /// <summary>
        /// Remove a contributor from the live thread.
        /// </summary>
        /// <param name="user">RedditUser</param>
        public async Task<bool> RemoveContributorAsync(RedditUser user)
        {
            var request = WebAgent.CreateRequest(RemoveContributorUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                id = user.Kind + "_" + user.Id
            });
            var response  = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Remove a contributor from the live thread.
        /// </summary>
        /// <param name="userName">reddit username.</param>
        public Task<bool> RemoveContributorAsync(string userName)
        {
            return RemoveContributorAsync(userName);
        }

        /// <summary>
        /// Report the live thread.  (Spam by default).
        /// </summary>
        /// <param name="reason">one of the following : "spam" (default), "vote-manipulation", "personal-information", "sexualizing-minors", "site-breaking"</param>
        public async Task<bool> ReportAsync(string reason = "spam")
        {
            var allowed = new List<string>() {
                "spam" ,
                "vote-manipulation" ,
                "personal-information" ,
                "sexualizing-minors" ,
                "site-breaking"
            };

            if (!allowed.Contains(reason))
            {
                var message = "Invalid report type.  Valid types are : ";
                for (int i = 0; i < allowed.Count; i++)
                {
                    message += "'" + allowed[i] + "'";
                    if (i != allowed.Count - 1)
                        message += ", ";
                }
                throw new InvalidOperationException(message);
            }
            
            var request = WebAgent.CreateRequest(ReportUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                type = reason
            });

            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Revoke an outstanding contributor invite.
        /// </summary>
        /// <param name="user">reddit user.</param>
        public async Task<bool> RevokeContributorInviteAsync(RedditUser user)
        {

            var request = WebAgent.CreateRequest(RevokeContributorInviteUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                id = RedditUser.KindPrefix + user.Id
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Revoke an outstanding contributor invite.
        /// </summary>
        /// <param name="name">reddit username</param>
        public async Task<bool> RevokeContributorInviteAsync(string name)
        {
            var redditUser = await RedditUser.GetUserAsync(WebAgent, name);
            return await RevokeContributorInviteAsync(redditUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Set contributor permissions on the live thread.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions">Reddit user</param>
        public async Task<bool> SetContributorPermissionsAsync(RedditUser user, LiveUpdateEventPermission permissions)
        {
            return await SetContributorPermissions(user.Name, permissions).ConfigureAwait(false);
        }

        /// <summary>
        /// Set contributor permissions on the live thread.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="permissions">Permissions to set.</param>
        public async Task<bool> SetContributorPermissions(string userName, LiveUpdateEventPermission permissions)
        {
            var request = WebAgent.CreateRequest(SetContributorPermissionUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                name = userName,
                type = "liveupdate_contributor",
                permissions = GetPermissionsString(permissions)
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Set permissions on a contributor who has been invited but has not accepted.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissions">Permissions to set.</param>
        public async Task<bool> SetInvitedContributorPermissionsAsync(RedditUser user, LiveUpdateEventPermission permissions)
        {
            return await SetInvitedContributorPermissionsAsync(user.Name, permissions).ConfigureAwait(false);
        }

        /// <summary>
        /// Set permissions on a contributor who has been invited but has not accepted.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="permissions">Permissions to set.</param>
        public async Task<bool> SetInvitedContributorPermissionsAsync(string userName, LiveUpdateEventPermission permissions)
        {
            var request = WebAgent.CreateRequest(SetContributorPermissionUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                name = userName,
                type = "liveupdate_contributor_invite",
                permissions = GetPermissionsString(permissions)
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Strike an update
        /// </summary>
        /// <param name="update">Update to strike</param>
        public async Task<bool> StrikeUpdateAsync(LiveUpdate update)
        {
            return await StrikeUpdateAsync(update.FullName).ConfigureAwait(false);
        }

        /// <summary>
        /// Strike an update
        /// </summary>
        /// <param name="fullName">Full name of the update to strike</param>
        public async Task<bool> StrikeUpdateAsync(string fullName)
        {
            var request = WebAgent.CreateRequest(StrikeUpdateUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                id = fullName
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        /// <summary>
        /// Make an update to the live thread
        /// </summary>
        /// <param name="markdown">markdown of the update</param>
        public async Task<bool> UpdateAsync(string markdown)
        {
            var request = WebAgent.CreateRequest(UpdateUrl, "POST");
            WebAgent.WritePostBody(request, new
            {
                api_type = "json",
                body = markdown
            });
            var response = await WebAgent.GetResponseAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        private string GetPermissionsString(LiveUpdateEventPermission input)
        {
            // settings of edit + close
            // -all,-close,+edit,-manage,+settings,-update


            if (input == LiveUpdateEventPermission.All)
                return "+all";

            if (input == LiveUpdateEventPermission.None)
                return "-all,-close,-edit,-manage,-settings,-update";

            var result = "-all,";

            if (input.HasFlag(LiveUpdateEventPermission.Close))
                result += "+close,";
            else
                result += "-close,";

            if (input.HasFlag(LiveUpdateEventPermission.Edit))
                result += "+edit,";
            else
                result += "-edit,";

            if (input.HasFlag(LiveUpdateEventPermission.Manage))
                result += "+manage,";
            else
                result += "-manage,";

            if (input.HasFlag(LiveUpdateEventPermission.Settings))
                result += "+settings,";
            else
                result += "-settings,";

            if (input.HasFlag(LiveUpdateEventPermission.Update))
                result += "+update,";
            else
                result += "-update,";

            if (result.EndsWith(","))
                result = result.Remove(result.Length - 1, 1);

            return result;
        }

        private class PermissionsConverter : JsonConverter
        {
            // https://github.com/reddit/reddit-plugin-liveupdate/blob/master/reddit_liveupdate/permissions.py
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var flags = (LiveUpdateEventPermission)value;
                writer.WriteStartArray();
                if (flags.HasFlag(LiveUpdateEventPermission.All))
                {
                    writer.WriteValue("all");
                    writer.WriteEndArray();
                    return;
                }

                if (flags.HasFlag(LiveUpdateEventPermission.None))
                {
                    writer.WriteValue("none");
                    writer.WriteEndArray();
                    return;
                }

                if (flags.HasFlag(LiveUpdateEventPermission.Edit))
                    writer.WriteValue("edit");

                if (flags.HasFlag(LiveUpdateEventPermission.Close))
                    writer.WriteValue("close");

                if (flags.HasFlag(LiveUpdateEventPermission.Manage))
                    writer.WriteValue("manage");

                if (flags.HasFlag(LiveUpdateEventPermission.Settings))
                    writer.WriteValue("settings");

                if (flags.HasFlag(LiveUpdateEventPermission.Update))
                    writer.WriteValue("update");

                writer.WriteEndArray();

            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader).ToObject<string[]>();

                var result = LiveUpdateEventPermission.None;
                var exit = false;
                foreach (var item in array)
                {
                    switch (item)
                    {
                        case "all":
                            result = LiveUpdateEventPermission.All;
                            exit = true;
                            break;
                        case "none":
                            result = LiveUpdateEventPermission.None;
                            exit = true;
                            break;
                        case "update":
                            result = result | LiveUpdateEventPermission.Update;
                            break;
                        case "manage":
                            result = result | LiveUpdateEventPermission.Manage;
                            break;
                        case "edit":
                            result = result | LiveUpdateEventPermission.Edit;
                            break;
                        case "settings":
                            result = result | LiveUpdateEventPermission.Settings;
                            break;
                        case "close":
                            result = result | LiveUpdateEventPermission.Close;
                            break;
                    }
                    if (exit)
                        break;
                }
                return result;
            }

            public override bool CanConvert(Type objectType) => true;
        }
    }
}
