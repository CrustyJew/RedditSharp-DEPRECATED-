using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Multi
{
    public class Multi
    {
        private Reddit Reddit { get; set; }
        private IWebAgent WebAgent { get; set; }

        #region Constant API URLs
        private const string GetCurrentUserMultiUrl = "/api/multi/mine";
        private const string GetPublicUserMultiUrl = "/api/multi/user/{0}";
        private const string GetMultiPathUrl = "/api/multi/{0}";
        private const string GetMultiDescriptionPathUrl = "/api/multi/{0}/description";
        private const string GetMultiSubUrl = "/api/multi/{0}/r/{1}";
        private const string PostMultiRenameUrl = "/api/multi/rename";
        private const string PutSubMultiUrl = "/api/multi/{0}/r/{1}";
        private const string CopyMultiUrl = "/api/multi/copy";

        #endregion


        public Multi(Reddit reddit, IWebAgent webAgent)
        {
            Reddit = reddit;
            WebAgent = webAgent;
        }

        /// <summary>
        /// Retrieve a list of the Multis belonging to the currently authenticated user
        /// </summary>
        /// <returns>A List of MultiData containing the authenticated user's Multis</returns>
        public List<MultiData> GetCurrentUsersMultis()
        {
            var request = WebAgent.CreateGet(GetCurrentUserMultiUrl);
            var response = request.GetResponse();
            var json = JToken.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
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
        public List<MultiData> GetPublicUserMultis(string username)
        {
            var request = WebAgent.CreateGet(string.Format(GetPublicUserMultiUrl,username));
            var response = request.GetResponse();
            var json = JToken.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
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
        public MultiData GetMultiByPath(string path)
        {
            var request = WebAgent.CreateGet(string.Format(GetMultiPathUrl, path));
            var response = request.GetResponse();
            var json = JToken.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
            var result = new MultiData(Reddit, json, WebAgent);
            return result;
        }

        /// <summary>
        /// Retrieve the description for the Multi based on the URL path given
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <returns>A MultiData containing the description for the found Multi</returns>
        public MultiData GetMultiDescription(string path)
        {
            var request = WebAgent.CreateGet(string.Format(GetMultiDescriptionPathUrl, path));
            var response = request.GetResponse();
            var json = JToken.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
            var result = new MultiData(Reddit, json, WebAgent, false);
            return result;
        }

        /// <summary>
        /// Retrieve the information for a given subreddit in a Multi
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <param name="subreddit">Subreddit name to get information for</param>
        /// <returns>A MultiSubs element containing the information for the searched subreddit</returns>
        public MultiSubs GetSubInformation(string path, string subreddit)
        {
            var request = WebAgent.CreateGet(string.Format(GetMultiSubUrl, path,subreddit));
            var response = request.GetResponse();
            var json = JToken.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
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
        public string RenameMulti(string displayName, string pathFrom, string pathTo)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreatePost(PostMultiRenameUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Adds a Subreddit to the given Multi
        /// </summary>
        /// <param name="path">URL Path of the Multi to update</param>
        /// <param name="subName">Name of the subreddit to add</param>
        /// <returns>A String containing the information of the updated Multi</returns>
        public string PutSubMulti(string path, string subName)
        {
            if (Reddit.User == null)
            { 
                throw new AuthenticationException("No user logged in.");
             }
            var request = WebAgent.CreatePut(string.Format(PutSubMultiUrl,path,subName));
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("name", subName);
            WebAgent.WritePostBody(stream, new
            {
                model = modelData,
                multipath = path,
                srname = subName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;

        }

        /// <summary>
        /// Updates the description for a given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to update</param>
        /// <param name="description">New description for the Multi</param>
        /// <returns>A string containing the updated information of the Multi</returns>
        public string PutMultiDescription(string path, string description)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreatePut(string.Format(GetMultiDescriptionPathUrl, path));
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("body_md", description);
            WebAgent.WritePostBody(stream, new
                {
                    model = modelData,
                    multipath = path,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Makes a copy of a Multi
        /// </summary>
        /// <param name="displayName">Display name for the new Multi</param>
        /// <param name="pathFrom">URL path to copy from</param>
        /// <param name="pathTo">URL path to copy to</param>
        /// <returns>A string containing the information of the new Multi</returns>
        public string CopyMulti(string displayName, string pathFrom, string pathTo)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreatePost(CopyMultiUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = Reddit.User.Modhash
            });
            stream.Close();

            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            
            return data;
        }

        /// <summary>
        /// Remove a Subreddit from the given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to edit</param>
        /// <param name="subname">Subreddit name to be removed</param>
        /// <returns>A string containing the updated information of the given Multi.</returns>
        public string DeleteSub(string path, string subname)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreateDelete(string.Format(GetMultiSubUrl, path, subname));
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
                {
                    multipath = path,
                    srname = subname,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Deletes a Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to Delete</param>
        /// <returns>A string containing the success code for deletion.</returns>
        public string DeleteMulti(string path)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in.");
            }
            var request = WebAgent.CreateDelete(string.Format(GetMultiPathUrl, path));
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                multipath = path,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
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
        public string PostMulti(MData m, string path)
        {
            if(Reddit.User == null)
            { 
                throw new AuthenticationException("No user logged in");
            }
            var request = WebAgent.CreatePost(string.Format(GetMultiPathUrl,path));
            var stream = request.GetRequestStream();
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
            WebAgent.WritePostBody(stream, new
                {
                    model = modelData,
                    multipath = path,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
            
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
        public string PutMulti(MData m, string path)
        {
            if (Reddit.User == null)
            {
                throw new AuthenticationException("No user logged in");
            }
            var request = WebAgent.CreatePut(string.Format(GetMultiPathUrl, path));
            var stream = request.GetRequestStream();
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
            WebAgent.WritePostBody(stream, new
            {
                model = modelData,
                multipath = path,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }
    }
}
