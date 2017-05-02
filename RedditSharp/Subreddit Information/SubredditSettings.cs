using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using RedditSharp.Things;
using System.Threading.Tasks;
using System.Net;

namespace RedditSharp
{
    /// <summary>
    /// Subreddit settings.
    /// </summary>
    public class SubredditSettings : RedditObject
    {
        private const string SiteAdminUrl = "/api/site_admin";
        private const string DeleteHeaderImageUrl = "/api/delete_sr_header";

        /// <summary>
        /// Parent subreddit.
        /// </summary>
        [JsonIgnore]
        public Subreddit Subreddit { get; private set; }

        /// <summary>
        /// Get the subreddit settings page.
        /// </summary>
        /// <param name="subreddit">A subreddit.</param>
        public SubredditSettings(Subreddit subreddit) : base(subreddit?.WebAgent)
        {
            Subreddit = subreddit;
            // Default settings, for use when reduced information is given
            AllowAsDefault = true;
            AllowImages = false;
            Domain = null;
            Sidebar = string.Empty;
            Language = "en";
            Title = Subreddit.DisplayName;
            WikiEditKarma = 100;
            WikiEditAge = 10;
            UseDomainCss = false;
            UseDomainSidebar = false;
            HeaderHoverText = string.Empty;
            NSFW = false;
            PublicDescription = string.Empty;
            WikiEditMode = WikiEditMode.None;
            SubredditType = SubredditType.Public;
            ShowThumbnails = true;
            ContentOptions = ContentOptions.All;
            SpamFilter = new SpamFilterSettings();
        }

        /// <summary>
        /// Get the subreddit settings page.
        /// </summary>
        /// <param name="subreddit">A subreddit.</param>
        /// <param name="json"></param>
        public SubredditSettings(Subreddit subreddit, JToken json) : this(subreddit)
        {
            var data = json["data"];
            AllowAsDefault = data["default_set"].ValueOrDefault<bool>();
            AllowImages = data["allow_images"].ValueOrDefault<bool>();
            Domain = data["domain"].ValueOrDefault<string>();
            Sidebar = WebUtility.HtmlDecode(data["description"].ValueOrDefault<string>() ?? string.Empty);
            Language = data["language"].ValueOrDefault<string>();
            Title = data["title"].ValueOrDefault<string>();
            WikiEditKarma = data["wiki_edit_karma"].ValueOrDefault<int>();
            UseDomainCss = data["domain_css"].ValueOrDefault<bool>();
            UseDomainSidebar = data["domain_sidebar"].ValueOrDefault<bool>();
            HeaderHoverText = data["header_hover_text"].ValueOrDefault<string>();
            NSFW = data["over_18"].ValueOrDefault<bool>();
            PublicDescription = WebUtility.HtmlDecode(data["public_description"].ValueOrDefault<string>() ?? string.Empty);
            SpamFilter = new SpamFilterSettings
            {
                LinkPostStrength = GetSpamFilterStrength(data["spam_links"].ValueOrDefault<string>()),
                SelfPostStrength = GetSpamFilterStrength(data["spam_selfposts"].ValueOrDefault<string>()),
                CommentStrength = GetSpamFilterStrength(data["spam_comments"].ValueOrDefault<string>())
            };
            if (data["wikimode"] != null)
            {
                var wikiMode = data["wikimode"].ValueOrDefault<string>();
                switch (wikiMode)
                {
                    case "disabled":
                        WikiEditMode = WikiEditMode.None;
                        break;
                    case "modonly":
                        WikiEditMode = WikiEditMode.Moderators;
                        break;
                    case "anyone":
                        WikiEditMode = WikiEditMode.All;
                        break;
                }
            }
            if (data["subreddit_type"] != null)
            {
                var type = data["subreddit_type"].ValueOrDefault<string>();
                switch (type)
                {
                    case "public":
                        SubredditType = SubredditType.Public;
                        break;
                    case "private":
                        SubredditType = SubredditType.Private;
                        break;
                    case "restricted":
                        SubredditType = SubredditType.Restricted;
                        break;
                }
            }
            ShowThumbnails = data["show_media"].ValueOrDefault<bool>();
            WikiEditAge = data["wiki_edit_age"].ValueOrDefault<int>();
            if (data["content_options"] != null)
            {
                var contentOptions = data["content_options"].ValueOrDefault<string>();
                switch (contentOptions)
                {
                    case "any":
                        ContentOptions = ContentOptions.All;
                        break;
                    case "link":
                        ContentOptions = ContentOptions.LinkOnly;
                        break;
                    case "self":
                        ContentOptions = ContentOptions.SelfOnly;
                        break;
                }
            }
        }

        /// <summary>
        /// Allow this subreddit to be included /r/all as well as the default and trending lists.
        /// </summary>
        public bool AllowAsDefault { get; set; }

        /// <summary>
        /// Domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Markdown of the sidebar.
        /// </summary>
        public string Sidebar { get; set; }

        /// <summary>
        /// A valid IETF language tag (underscore supported).
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Subreddit title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Subreddit karma required to edit and create wiki pages.
        /// </summary>
        public int WikiEditKarma { get; set; }

        /// <summary>
        /// Set to true to use domain css.
        /// </summary>
        public bool UseDomainCss { get; set; }

        /// <summary>
        /// Set to true to use domain sidebar.
        /// </summary>
        public bool UseDomainSidebar { get; set; }

        /// <summary>
        /// Header hover text.
        /// </summary>
        public string HeaderHoverText { get; set; }

        /// <summary>
        /// Viewers must be over eighteen years old
        /// </summary>
        public bool NSFW { get; set; }

        /// <summary>
        /// Public description text.
        /// </summary>
        public string PublicDescription { get; set; }

        /// <summary>
        /// Wiki edit mode.
        /// </summary>
        public WikiEditMode WikiEditMode { get; set; }

        /// <summary>
        /// Subreddit type.
        /// </summary>
        public SubredditType SubredditType { get; set; }

        /// <summary>
        /// Set to true to show thumbnail images of content.
        /// </summary>
        public bool ShowThumbnails { get; set; }

        /// <summary>
        /// Account age (days) required to edit and create wiki pages.
        /// </summary>
        public int WikiEditAge { get; set; }

        /// <summary>
        /// Content options.
        /// </summary>
        public ContentOptions ContentOptions { get; set; }

        /// <summary>
        /// Spam filter settings.
        /// </summary>
        public SpamFilterSettings SpamFilter { get; set; }

        /// <summary>
        /// Set to bool to allow images
        /// </summary>
        public bool AllowImages { get; set; }

        /// <summary>
        /// Update the subreddit settings.
        /// </summary>
        public async Task UpdateSettings()
        {
            string link_type;
            string type;
            string wikimode;
            switch (ContentOptions)
            {
                case ContentOptions.All:
                    link_type = "any";
                    break;
                case ContentOptions.LinkOnly:
                    link_type = "link";
                    break;
                default:
                    link_type = "self";
                    break;
            }
            switch (SubredditType)
            {
                case SubredditType.Public:
                    type = "public";
                    break;
                case SubredditType.Private:
                    type = "private";
                    break;
                default:
                    type = "restricted";
                    break;
            }
            switch (WikiEditMode)
            {
                case WikiEditMode.All:
                    wikimode = "anyone";
                    break;
                case WikiEditMode.Moderators:
                    wikimode = "modonly";
                    break;
                default:
                    wikimode = "disabled";
                    break;
            }
            await WebAgent.Post(SiteAdminUrl, new
            {
                allow_top = AllowAsDefault,
                allow_images = AllowImages,
                description = Sidebar,
                domain = Domain,
                lang = Language,
                link_type,
                over_18 = NSFW,
                public_description = PublicDescription,
                show_media = ShowThumbnails,
                sr = Subreddit.FullName,
                title = Title,
                type,
                wiki_edit_age = WikiEditAge,
                wiki_edit_karma = WikiEditKarma,
                wikimode,
                spam_links = SpamFilter == null ? null : SpamFilter.LinkPostStrength.ToString().ToLowerInvariant(),
                spam_selfposts = SpamFilter == null ? null : SpamFilter.SelfPostStrength.ToString().ToLowerInvariant(),
                spam_comments = SpamFilter == null ? null : SpamFilter.CommentStrength.ToString().ToLowerInvariant(),
                api_type = "json"
            }, "header-title", HeaderHoverText).ConfigureAwait(false);
        }

        /// <summary>
        /// Resets the subreddit's header image to the Reddit logo
        /// </summary>
        public async Task ResetHeaderImage()
        {
            await WebAgent.Post(DeleteHeaderImageUrl, new
            {
                r = Subreddit.Name
            }).ConfigureAwait(false);
        }

        private SpamFilterStrength GetSpamFilterStrength(string rawValue)
        {
            switch(rawValue)
            {
                case "low":
                    return SpamFilterStrength.Low;
                case "high":
                    return SpamFilterStrength.High;
                case "all":
                    return SpamFilterStrength.All;
                default:
                    return SpamFilterStrength.High;
            }
        }
    }

    /// <summary>
    /// Rules for editing the wiki.
    /// </summary>
    public enum WikiEditMode
    {
        /// <summary>
        /// Wiki is disabled for all users except mods.
        /// </summary>
        None,
        /// <summary>
        /// Only mods, approved wiki contributors, or those on a page's edit list may edit.
        /// </summary>
        Moderators,
        /// <summary>
        /// Anyone who can submit to the subreddit may edit.
        /// </summary>
        All
    }

    /// <summary>
    /// Type of Subreddit.
    /// </summary>
    public enum SubredditType
    {
        /// <summary>
        /// Anyone can view and submit.
        /// </summary>
        Public,
        /// <summary>
        /// Anyone can view, but only some are approved to submit links.
        /// </summary>
        Restricted,
        /// <summary>
        /// Only approved members can view and submit.
        /// </summary>
        Private
    }

    /// <summary>
    /// Content Options.
    /// </summary>
    public enum ContentOptions
    {
        /// <summary>
        /// Any link type is allowed to be submitted.
        /// </summary>
        All,
        /// <summary>
        /// Only links to external sites are allowed to be submitted.
        /// </summary>
        LinkOnly,
        /// <summary>
        /// Only text/self posts are allowed to be submitted.
        /// </summary>
        SelfOnly
    }

    /// <summary>
    /// Spam filter strength.
    /// </summary>
    public enum SpamFilterStrength
    {
        /// <summary>
        /// Low disables most filtering.
        /// </summary>
        Low,
        /// <summary>
        /// High is the standard filter.
        /// </summary>
        High,
        /// <summary>
        /// Filter every post initially and they will need to be approved manually to be visible.
        /// </summary>
        All
    }
}
