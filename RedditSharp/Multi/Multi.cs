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
        private Reddit reddit { get; set; }
        private IWebAgent webAgent { get; set; }

        #region Constant API URLs
        private const string getCurrentUserMultiUrl = "/api/multi/mine";
        private const string getPublicUserMultiUrl = "/api/multi/user/{0}";
        private const string getMultiPathUrl = "/api/multi/{0}";
        private const string getMultiDescriptionPathUrl = "/api/multi/{0}/description";
        private const string getMultiSubUrl = "/api/multi/{0}/r/{1}";
        private const string postMultiRenameUrl = "/api/multi/rename";
        private const string putSubMultiUrl = "/api/multi/{0}/r/{1}";
        private const string copyMultiUrl = "/api/multi/copy";

        #endregion


        public Multi(Reddit reddit2, IWebAgent webAgent2)
        {
            reddit = reddit2;
            webAgent = webAgent2;
        }

        /// <summary>
        /// Retrieve a list of the Multis belonging to the currently authenticated user
        /// </summary>
        /// <returns>A List of MultiData containing the authenticated user's Multis</returns>
        public List<MultiData> getCurrentUsersMultis()
        {
            var request = webAgent.CreateGet(getCurrentUserMultiUrl);
            var response = request.GetResponse();
            var json = JToken.Parse(webAgent.GetResponseString(response.GetResponseStream()));
            List<MultiData> results = new List<MultiData>();
            for (int i = 0; i < json.Count();i++)
            {
                results.Add(new MultiData(reddit,json[i],webAgent));
            }
            return results;
        }

        /// <summary>
        /// Retrieve a list of public Multis belonging to a given user
        /// </summary>
        /// <param name="username">Username to search</param>
        /// <returns>A list of MultiData containing the public Multis of the searched user</returns>
        public List<MultiData> getPublicUserMultis(string username)
        {
            var request = webAgent.CreateGet(string.Format(getPublicUserMultiUrl,username));
            var response = request.GetResponse();
            var json = JToken.Parse(webAgent.GetResponseString(response.GetResponseStream()));
            List<MultiData> results = new List<MultiData>();
            for (int i = 0; i < json.Count();i++)
            {
                results.Add(new MultiData(reddit, json[i], webAgent));
            }
            return results;
        }

        /// <summary>
        /// Retrieve the information of a Multi based on the URL path given
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <returns>A MultiData containing the information for the found Multi</returns>
        public MultiData getMultiByPath(string path)
        {
            var request = webAgent.CreateGet(string.Format(getMultiPathUrl, path));
            var response = request.GetResponse();
            var json = JToken.Parse(webAgent.GetResponseString(response.GetResponseStream()));
            var result = new MultiData(reddit, json, webAgent);
            return result;
        }

        /// <summary>
        /// Retrieve the description for the Multi based on the URL path given
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <returns>A MultiData containing the description for the found Multi</returns>
        public MultiData getMultiDescription(string path)
        {
            var request = webAgent.CreateGet(string.Format(getMultiDescriptionPathUrl, path));
            var response = request.GetResponse();
            var json = JToken.Parse(webAgent.GetResponseString(response.GetResponseStream()));
            var result = new MultiData(reddit, json, webAgent, false);
            return result;
        }

        /// <summary>
        /// Retrieve the information for a given subreddit in a Multi
        /// </summary>
        /// <param name="path">URL path to use</param>
        /// <param name="subreddit">Subreddit name to get information for</param>
        /// <returns>A MultiSubs element containing the information for the searched subreddit</returns>
        public MultiSubs getSubInformation(string path, string subreddit)
        {
            var request = webAgent.CreateGet(string.Format(getMultiSubUrl, path,subreddit));
            var response = request.GetResponse();
            var json = JToken.Parse(webAgent.GetResponseString(response.GetResponseStream()));
            var result = new MultiSubs(reddit, json, webAgent);
            return result;
        }

        /// <summary>
        /// Rename a Multi based on the given information
        /// </summary>
        /// <param name="displayName">New Display name for the Multi</param>
        /// <param name="pathFrom">Original URL path of the Multi</param>
        /// <param name="pathTo">New URL path of the Multi</param>
        /// <returns>A String containing the new Multi information</returns>
        public string renameMulti(string displayName, string pathFrom, string pathTo)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreatePost(postMultiRenameUrl);
            var stream = request.GetRequestStream();
            webAgent.WritePostBody(stream, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Adds a Subreddit to the given Multi
        /// </summary>
        /// <param name="path">URL Path of the Multi to update</param>
        /// <param name="subName">Name of the subreddit to add</param>
        /// <returns>A String containing the information of the updated Multi</returns>
        public string putSubMulti(string path, string subName)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreateRequest(string.Format(putSubMultiUrl,path,subName),"PUT");
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("name", subName);
            webAgent.WritePostBody(stream, new
            {
                model = modelData,
                multipath = path,
                srname = subName,
                uh = reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            return data;

        }

        /// <summary>
        /// Updates the description for a given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to update</param>
        /// <param name="description">New description for the Multi</param>
        /// <returns>A string containing the updated information of the Multi</returns>
        public string putMultiDescription(string path, string description)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreateRequest(string.Format(getMultiDescriptionPathUrl, path), "PUT");
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("body_md", description);
            webAgent.WritePostBody(stream, new
                {
                    model = modelData,
                    multipath = path,
                    uh = reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Makes a copy of a Multi
        /// </summary>
        /// <param name="displayName">Display name for the new Multi</param>
        /// <param name="pathFrom">URL path to copy from</param>
        /// <param name="pathTo">URL path to copy to</param>
        /// <returns>A string containing the information of the new Multi</returns>
        public string copyMulti(string displayName, string pathFrom, string pathTo)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreatePost(copyMultiUrl);
            var stream = request.GetRequestStream();
            webAgent.WritePostBody(stream, new
            {
                display_name = displayName,
                from = pathFrom,
                to = pathTo,
                uh = reddit.User.Modhash
            });
            stream.Close();

            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            
            return data;
        }

        /// <summary>
        /// Remove a Subreddit from the given Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to edit</param>
        /// <param name="subname">Subreddit name to be removed</param>
        /// <returns>A string containing the updated information of the given Multi.</returns>
        public string deleteSub(string path, string subname)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreateRequest(string.Format(getMultiSubUrl, path, subname),"DELETE");
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            webAgent.WritePostBody(stream, new
                {
                    multipath = path,
                    srname = subname,
                    uh = reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        /// <summary>
        /// Deletes a Multi
        /// </summary>
        /// <param name="path">URL path of the Multi to Delete</param>
        /// <returns>A string containing the success code for deletion.</returns>
        public string deleteMulti(string path)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = webAgent.CreateRequest(string.Format(getMultiPathUrl, path), "DELETE");
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            webAgent.WritePostBody(stream, new
            {
                multipath = path,
                uh = reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
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
        public string postMulti(string description, string displayname, string iconname, string keycolor, string[] subreddits, string visibility, string weightingscheme, string path)
        {
            if(reddit.User == null)
                throw new AuthenticationException("No user logged in");
            var request = webAgent.CreatePost(string.Format(getMultiPathUrl,path));
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("description_md",description);
            modelData.Add("display_name",displayname);
            modelData.Add("icon_name",iconname);
            modelData.Add("key_color",keycolor);
            JArray subData = new JArray();
            for(int i = 0; i < subreddits.Count();i++)
            {
                JObject sub = new JObject();
                sub.Add("name",subreddits[i]);
                subData.Add(sub);
            }
            modelData.Add("subreddits",subData);
            modelData.Add("visibility",visibility);
            modelData.Add("weighting_scheme",weightingscheme);
            webAgent.WritePostBody(stream, new
                {
                    model = modelData,
                    multipath = path,
                    uh = reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
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
        public string putMulti(string description, string displayname, string iconname, string keycolor, string[] subreddits, string visibility, string weightingscheme, string path)
        {
            if (reddit.User == null)
                throw new AuthenticationException("No user logged in");
            var request = webAgent.CreateRequest(string.Format(getMultiPathUrl, path),"PUT");
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            JObject modelData = new JObject();
            modelData.Add("description_md", description);
            modelData.Add("display_name", displayname);
            modelData.Add("icon_name", iconname);
            modelData.Add("key_color", keycolor);
            JArray subData = new JArray();
            for (int i = 0; i < subreddits.Count(); i++)
            {
                JObject sub = new JObject();
                sub.Add("name", subreddits[i]);
                subData.Add(sub);
            }
            modelData.Add("subreddits", subData);
            modelData.Add("visibility", visibility);
            modelData.Add("weighting_scheme", weightingscheme);
            webAgent.WritePostBody(stream, new
            {
                model = modelData,
                multipath = path,
                uh = reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = webAgent.GetResponseString(response.GetResponseStream());
            return data;
        }
    }
}
