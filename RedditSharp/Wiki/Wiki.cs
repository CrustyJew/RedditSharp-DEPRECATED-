using RedditSharp.Things;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedditSharp
{

    /// <summary>
    /// A subreddit wiki.
    /// </summary>
    public class Wiki : RedditObject
    {
        /// <summary>
        /// Name of subreddit of the wiki
        /// </summary>
        public string SubredditName { get; private set; }

        #region constants
        private string GetWikiPageUrl(string page, string version) => $"/r/{SubredditName}/wiki/{page}.json?v={version}";
        private string GetWikiPagesUrl => $"/r/{SubredditName}/wiki/pages.json";
        private string WikiPageEditUrl => $"/r/{SubredditName}/api/wiki/edit";
        private string HideWikiPageUrl => $"/r/{SubredditName}/api/wiki/hide";
        private string RevertWikiPageUrl => $"/r/{SubredditName}/api/wiki/revert";
        private string WikiPageAllowEditorAddUrl => $"/r/{SubredditName}/api/wiki/alloweditor/add";
        private string WikiPageAllowEditorDelUrl => $"/r/{SubredditName}/api/wiki/alloweditor/del";
        private string WikiPageSettingsUrl(string page) => $"/r/{SubredditName}/wiki/settings/{page}.json";
        private string WikiRevisionsUrl => $"/r/{SubredditName}/wiki/revisions.json";
        private string WikiPageRevisionsUrl(string page) => $"/r/{SubredditName}/wiki/revisions/{page}.json";
        private string WikiPageDiscussionsUrl(string page) => $"/r/{SubredditName}/wiki/discussions/{page}.json";
        #endregion

        /// <summary>
        /// Get a list of wiki page names for this subreddit.
        /// </summary>
        public async Task<IEnumerable<string>> GetPageNamesAsync()
        {
            var json = await WebAgent.Get(GetWikiPagesUrl).ConfigureAwait(false);
            return json["data"].Values<string>();
        }

        /// <summary>
        /// Get a list of revisions for this wiki.
        /// </summary>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<WikiPageRevision> GetRevisions(int max = -1) => Listing<WikiPageRevision>.Create(WebAgent, WikiRevisionsUrl, max, 100);

        /// <summary>
        /// Creates a new refrence to a subreddit's Wiki.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="subredditName"></param>
        public Wiki(IWebAgent agent, string subredditName):base(agent)
        {
            subredditName = System.Text.RegularExpressions.Regex.Replace(subredditName, "(r/|/)", "");
            SubredditName = subredditName;
        }


        /// <summary>
        /// Get a wiki page
        /// </summary>
        /// <param name="page">wiki page name</param>
        /// <param name="version">page version</param>
        /// <returns></returns>
        public async Task<WikiPage> GetPageAsync(string page, string version = null)
        {
            var json = await WebAgent.Get(GetWikiPageUrl(page, version)).ConfigureAwait(false);
            return new WikiPage(WebAgent, json["data"]);
        }

        #region Settings

        /// <summary>
        /// Get wiki settings for specified wiki page.
        /// </summary>
        /// <param name="name">wiki page</param>
        /// <returns></returns>
        public async Task<WikiPageSettings> GetPageSettingsAsync(string name)
        {
            var json = await WebAgent.Get(WikiPageSettingsUrl(name)).ConfigureAwait(false);
            return new WikiPageSettings(WebAgent, json["data"]);
        }

        /// <summary>
        /// Set settings for the specified wiki page.
        /// </summary>
        /// <param name="name">wiki page</param>
        /// <param name="listed">public listing or not</param>
        /// <param name="editMode"><see cref="WikiPageSettings.WikiPagePermissionLevel"/>editing permissions</param>
        public async Task SetPageSettingsAsync(string name, bool listed, WikiPageSettings.WikiPagePermissionLevel editMode)
        {
            await WebAgent.Post(WikiPageSettingsUrl(name), new
            {
                page = name,
                permlevel = (int)editMode,
                listed = listed
            }).ConfigureAwait(false);
        }
        #endregion

        #region Revisions

        /// <summary>
        /// Get a list of revisions for a give wiki page.
        /// </summary>
        /// <param name="page">wiki page</param>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<WikiPageRevision> GetPageRevisions(string page, int max = -1) =>  Listing<WikiPageRevision>.Create(WebAgent, WikiPageRevisionsUrl(page), max, 100);

        #endregion

        #region Discussions

        /// <summary>
        /// Get a list of discussions about this wiki page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="max">Maximum number of records to return.  -1 for unlimited.</param>
        public Listing<Post> GetPageDiscussions(string page, int max = -1) => Listing<Post>.Create(WebAgent, WikiPageDiscussionsUrl(page), max, 100);
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
                page = page
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
            await WebAgent.Post(WikiPageEditUrl, param, addParams.ToArray()).ConfigureAwait(false);
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
                revision = revision
            }).ConfigureAwait(false);
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
                revision = revision
            }).ConfigureAwait(false);
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
                username = username
            }).ConfigureAwait(false);
        }

    }
}
