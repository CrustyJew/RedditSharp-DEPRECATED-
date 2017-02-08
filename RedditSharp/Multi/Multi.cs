using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Multi
{
    public class Multi : RedditObject
    {
        #region Constant API URLs
        private const string GetCurrentUserMultiUrl = "/api/multi/mine";
        private string GetPublicUserMultiUrl(string user) => $"/api/multi/user/{user}";
        private string GetMultiPathUrl(string path) => $"/api/multi/{path}";
        private string GetMultiDescriptionPathUrl(string path) => $"/api/multi/{path}/description";
        private string GetMultiSubUrl(string path, string subredddit) => "/api/multi/{path}/r/{1}";
        private const string PostMultiRenameUrl = "/api/multi/rename";
        private string PutSubMultiUrl(string path, string subreddit) => $"/api/multi/{path}/r/{subreddit}";
        private const string CopyMultiUrl = "/api/multi/copy";
        #endregion

        public Multi(Reddit reddit) : base(reddit) {
        }

        /// <summary>
        /// Retrieve a list of the Multis belonging to the currently authenticated user
        /// </summary>
        /// <returns>A List of MultiData containing the authenticated user's Multis</returns>
        public async Task<IList<MultiData>> GetCurrentUsersMultisAsync()
        {
            var request = WebAgent.CreateGet(GetCurrentUserMultiUrl);
            var json = await WebAgent.ExecuteRequestAsync(request);

            List<MultiData> results = new List<MultiData>();
            foreach (var r in json)
            {
                results.Add(new MultiData(Reddit,r,WebAgent));
            }
            return results;
        }

        /// <summary>
        /// Retrieve a list of public Multis belonging to a given user
        /// </summary>
        /// <param name="username">Username to search</param>
        /// <returns>A list of MultiData containing the public Multis of the searched user</returns>
        public async Task<IList<MultiData>> GetPublicUserMultisAsync(string username)
        {
            var request = WebAgent.CreateGet(GetPublicUserMultiUrl(username));
            var json = await WebAgent.ExecuteRequestAsync(request);

            List<MultiData> results = new List<MultiData>();
            foreach (var r in json)
            {
                results.Add(new MultiData(Reddit, r, WebAgent));
            }
            return results;
        }

        /// <summary>
        /// Retrieve the information of a Multi based on the URL path given
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <returns>A MultiData containing the information for the found Multi</returns>
        public async Task<MultiData> GetMultiByPathAsync(string path)
        {
            var request = WebAgent.CreateGet(GetMultiPathUrl(path));
            var json = await WebAgent.ExecuteRequestAsync(request);
            var result = new MultiData(Reddit, json, WebAgent);
            return result;
        }

        /// <summary>
        /// Retrieve the description for the Multi based on the URL path given
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <returns>A MultiData containing the description for the found Multi</returns>
        public async Task<MultiData> GetMultiDescriptionAsync(string path)
        {
            var request = WebAgent.CreateGet(GetMultiDescriptionPathUrl(path));
            var json = await WebAgent.ExecuteRequestAsync(request);
            var result = new MultiData(Reddit, json, WebAgent, false);
            return result;
        }

        /// <summary>
        /// Retrieve the information for a given subreddit in a Multi
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <param name="subreddit">Subreddit name to get information for</param>
        /// <returns>A MultiSubs element containing the information for the searched subreddit</returns>
        public async Task<MultiSubs> GetSubInformationAsync(string path, string subreddit)
        {
            var request = WebAgent.CreateGet(GetMultiSubUrl(path, subreddit));
            var json = await WebAgent.ExecuteRequestAsync(request);
            var result = new MultiSubs(Reddit, json, WebAgent);
            return result;
        }

        /// <summary>
        /// Rename a Multi based on the given information
        /// </summary>
        /// <param name="displayName">New Display name for the Multi</param>
        /// <param name="pathFrom">Original URL path of the Multi</param>
        /// <param name="pathTo">New URL path of the Multi</param>
        /// <returns>A String containing the new Multi information</returns>
        public async Task<string> RenameMultiAsync(string displayName, string pathFrom, string pathTo)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreatePost(PostMultiRenameUrl);
            WebAgent.WritePostBody(request, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = Reddit.User.Modhash
            });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Adds a Subreddit to the given Multi
        /// </summary>
        /// <param name="path">URL Path of the Multi to update</param>
        /// <param name="subName">Name of the subreddit to add</param>
        /// <returns>A String containing the information of the updated Multi</returns>
        public async Task<string> PutSubMultiAsync(string path, string subName)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
             }

            var request = WebAgent.CreateRequest(PutSubMultiUrl(path ,subName),"PUT");
            JObject modelData = new JObject();
            modelData.Add("name", subName);


            WebAgent.WritePostBody(request, new
            {
                model = modelData,
                multipath = path,
                srname = subName,
                uh = Reddit.User.Modhash
            });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Updates the description for a given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to update</param>
        /// <param name="description">New description for the Multi</param>
        /// <returns>A string containing the updated information of the Multi</returns>
        public async Task<string> PutMultiDescriptionAsync(string path, string description)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreateRequest(GetMultiDescriptionPathUrl(path),"PUT");
            JObject modelData = new JObject();
            modelData.Add("body_md", description);
            WebAgent.WritePostBody(request, new
                {
                    model = modelData,
                    multipath = path,
                    uh = Reddit.User.Modhash
                });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Makes a copy of a Multi
        /// </summary>
        /// <param name="displayName">Display name for the new Multi</param>
        /// <param name="pathFrom">URL path to copy from</param>
        /// <param name="pathTo">URL path to copy to</param>
        /// <returns>A string containing the information of the new Multi</returns>
        public async Task<string> CopyMultiAsync(string displayName, string pathFrom, string pathTo)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreatePost(CopyMultiUrl);
            WebAgent.WritePostBody(request, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = Reddit.User.Modhash
            });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Remove a Subreddit from the given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to edit</param>
        /// <param name="subname">Subreddit name to be removed</param>
        /// <returns>A string containing the updated information of the given Multi.</returns>
        public async Task<string> DeleteSubAsync(string path, string subname)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreateRequest(GetMultiSubUrl(path, subname),"DELETE");
            WebAgent.WritePostBody(request, new
                {
                    multipath = path,
                    srname = subname,
                    uh = Reddit.User.Modhash
                });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Deletes a Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to Delete</param>
        /// <returns>A string containing the success code for deletion.</returns>
        public async Task<string> DeleteMultiAsync(string path)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreateRequest(GetMultiPathUrl(path),"DELETE");
            WebAgent.WritePostBody(request, new
            {
                multipath = path,
                uh = Reddit.User.Modhash
            });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Create a new Multi for the authenticated user
        /// </summary>
        /// <param name="description">Multi Description</param>
        /// <param name="displayname">Multi Display Name</param>
        /// <param name="iconname">Icon Name (must be one of the default values)</param>
        /// <param name="keycolor">Hex Code for the desired color</param>
        /// <param name="subreddits">Array of Subreddit names to add</param>
        /// <param name="visibility">Visibility state for the Multi</param>
        /// <param name="weightingscheme">Weighting Scheme for the Multi</param>
        /// <param name="path">Desired URL path for the Multi</param>
        /// <returns>A string containing the information for the newly created Multi or a status of (409) if the Multi already exists</returns>
        public async Task<string> PostMultiAsync(MData m, string path)
        {
            if(Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in");
            }
            var request = WebAgent.CreatePost(GetMultiPathUrl(path));
            JObject modelData = new JObject();
            modelData.Add("description_md",m.DescriptionMD);
            modelData.Add("display_name",m.DisplayName);
            modelData.Add("icon_name",m.IconName);
            modelData.Add("key_color",m.KeyColor);
            JArray subData = new JArray();
            foreach(var s in m.Subreddits)
            {
                JObject sub = new JObject();
                sub.Add("name",s.Name);
                subData.Add(sub);
            }
            modelData.Add("subreddits",subData);
            modelData.Add("visibility",m.Visibility);
            modelData.Add("weighting_scheme",m.WeightingScheme);
            WebAgent.WritePostBody(request, new
                {
                    model = modelData,
                    multipath = path,
                    uh = Reddit.User.Modhash
                });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Create or update a  Multi for the authenticated user
        /// </summary>
        /// <param name="description">Multi Description</param>
        /// <param name="displayname">Multi Display Name</param>
        /// <param name="iconname">Icon Name (must be one of the default values)</param>
        /// <param name="keycolor">Hex Code for the desired color</param>
        /// <param name="subreddits">Array of Subreddit names to add</param>
        /// <param name="visibility">Visibility state for the Multi</param>
        /// <param name="weightingscheme">Weighting Scheme for the Multi</param>
        /// <param name="path">Desired URL path for the Multi</param>
        /// <returns>A string containing the information for the newly created or updated Multi or a status of (409) if the Multi already exists</returns>
        public async Task<string> PutMultiAsync(MData m, string path)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in");
            }
            var request = WebAgent.CreateRequest(GetMultiPathUrl(path),"PUT");
            JObject modelData = new JObject();
            modelData.Add("description_md", m.DescriptionMD);
            modelData.Add("display_name", m.DisplayName);
            modelData.Add("icon_name", m.IconName);
            modelData.Add("key_color", m.KeyColor);
            JArray subData = new JArray();
            foreach (var s in m.Subreddits)
            {
                JObject sub = new JObject();
                sub.Add("name", s.Name);
                subData.Add(sub);
            }
            modelData.Add("subreddits", subData);
            modelData.Add("visibility", m.Visibility);
            modelData.Add("weighting_scheme", m.WeightingScheme);
            WebAgent.WritePostBody(request, new
            {
                model = modelData,
                multipath = path,
                uh = Reddit.User.Modhash
            });

            var response = await WebAgent.GetResponseAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
