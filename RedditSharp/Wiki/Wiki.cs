using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System.Collections.Generic;

namespace RedditSharp
{
    using System;
    using System.Threading.Tasks;

    public class Wiki
    {
        private Subreddit Subreddit { get; set; }
        private Reddit Reddit => Subreddit?.Reddit;
        private IWebAgent WebAgent => Reddit?.WebAgent;

        #region constants
        private string GetWikiPageUrl(string page, string version) => $"/r/{Subreddit.Name}/wiki/{page}.json?v={version}";
        private string GetWikiPagesUrl => $"/r/{Subreddit.Name}/wiki/pages.json";
        private string WikiPageEditUrl => $"/r/{Subreddit.Name}/api/wiki/edit";
        private string HideWikiPageUrl => $"/r/{Subreddit.Name}/api/wiki/hide";
        private string RevertWikiPageUrl => $"/r/{Subreddit.Name}/api/wiki/revert";
        private string WikiPageAllowEditorAddUrl => $"/r/{Subreddit.Name}/api/wiki/alloweditor/add";
        private string WikiPageAllowEditorDelUrl => $"/r/{Subreddit.Name}/api/wiki/alloweditor/del";
        private string WikiPageSettingsUrl(string page) => $"/r/{Subreddit.Name}/wiki/settings/{page}.json";
        private string WikiRevisionsUrl => $"/r/{Subreddit.Name}/wiki/revisions.json";
        private string WikiPageRevisionsUrl(string page) => $"/r/{Subreddit.Name}/wiki/revisions/{page}.json";
        private string WikiPageDiscussionsUrl(string page) => $"/r/{Subreddit.Name}/wiki/discussions/{page}.json";
        #endregion

        /// <summary>
        /// Get a list of wiki page names for this subreddit.
        /// </summary>
        public async Task<IEnumerable<string>> GetPageNamesAsync()
        {
            var json = await WebAgent.Get(GetWikiPagesUrl);
            return json["data"].Values<string>();
        }

        /// <summary>
        /// Get a list of revisions for this wiki.
        /// </summary>
        public Listing<WikiPageRevision> Revisions => new Listing<WikiPageRevision>(Reddit, WikiRevisionsUrl);

        protected internal Wiki(Subreddit subreddit)
        {
            Subreddit = subreddit;
        }

        /// <summary>
        /// Get a wiki page
        /// </summary>
        /// <param name="page">wiki page name</param>
        /// <param name="version">page version</param>
        /// <returns></returns>
        public async Task<WikiPage> GetPageAsync(string page, string version = null)
        {
            var json = await WebAgent.Get(GetWikiPageUrl(page, version));
            return new WikiPage(Reddit, json["data"]);
        }

        #region Settings

        /// <summary>
        /// Get wiki settings for specified wiki page.
        /// </summary>
        /// <param name="name">wiki page</param>
        /// <returns></returns>
        public async Task<WikiPageSettings> GetPageSettingsAsync(string name)
        {
            var json = await WebAgent.Get(WikiPageSettingsUrl(name));
            return new WikiPageSettings(Reddit, json["data"]);
        }

        /// <summary>
        /// Set settings for the specified wiki page.
        /// </summary>
        /// <param name="name">wiki page</param>
        /// <param name="settings">settings</param>
        public async Task SetPageSettingsAsync(string name, WikiPageSettings settings)
        {
            await WebAgent.Post(WikiPageSettingsUrl(name), new
            {
                page = name,
                permlevel = settings.PermLevel,
                listed = settings.Listed,
                uh = Reddit.User.Modhash
            });
        }
        #endregion

        #region Revisions

        /// <summary>
        /// Get a list of revisions for a give wiki page.
        /// </summary>
        /// <param name="page">wiki page</param>
        /// <returns></returns>
        public Listing<WikiPageRevision> GetPageRevisions(string page) => new Listing<WikiPageRevision>(Reddit, WikiPageRevisionsUrl(page));

        #endregion

        #region Discussions

        /// <summary>
        /// Get a list of discussions about this wiki page.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Listing<Post> GetPageDiscussions(string page) => new Listing<Post>(Reddit, WikiPageDiscussionsUrl(page));
        #endregion

        /// <summary>
        /// Edit a wiki page.
        /// </summary>
        /// <param name="page">wiki page</param>
        /// <param name="content">new content</param>
        /// <param name="previous">previous</param>
        /// <param name="reason">reason for edit</param>
        public async Task EditPageAsync(string page, string content, string previous = null, string reason = null)
        {
            dynamic param = new
            {
                content = content,
                page = page,
                uh = Reddit.User.Modhash
            };
            List<string> addParams = new List<string>();
            if (previous != null)
            {
                addParams.Add("previous");
                addParams.Add(previous);
            }
            if (reason != null)
            {
                addParams.Add("reason");
                addParams.Add(reason);
            }
            await WebAgent.Post(WikiPageEditUrl, param, addParams.ToArray());
        }

        /// <summary>
        /// Hide the specified wiki page.
        /// </summary>
        /// <param name="page">wiki page.</param>
        /// <param name="revision">reason for revision.</param>
        public async Task HidePageAsync(string page, string revision)
        {
            await WebAgent.Post(HideWikiPageUrl, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
        }

        /// <summary>
        /// Revert a page to a specific version.
        /// </summary>
        /// <param name="page">wiki page</param>
        /// <param name="revision">page version</param>
        public async Task RevertPageAsync(string page, string revision)
        {
            await WebAgent.Post(RevertWikiPageUrl, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
        }

        /// <summary>
        /// Set the page editor for a given page.
        /// </summary>
        /// <param name="page">wiki page</param>
        /// <param name="username"></param>
        /// <param name="allow"></param>
        public async Task SetPageEditorAsync(string page, string username, bool allow)
        {
            await WebAgent.Post(allow ? WikiPageAllowEditorAddUrl : WikiPageAllowEditorDelUrl, new
            {
                page = page,
                username = username,
                uh = Reddit.User.Modhash
            });
        }

    }
}
