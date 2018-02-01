using Newtonsoft.Json.Linq;
using RedditSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace RedditSharp
{
    public static class ToolBoxUserNotes
    {
        private const string ToolBoxUserNotesWiki = "/r/{0}/wiki/usernotes";
        public static async Task<IEnumerable<TBUserNote>> GetUserNotesAsync(IWebAgent webAgent, string subName)
        {
            var reqResponse = await webAgent.Get(string.Format(ToolBoxUserNotesWiki, subName)).ConfigureAwait(false);
            var response = JObject.Parse(reqResponse["data"]["content_md"].Value<string>());
            int version = response["ver"].Value<int>();
            if(version < 6) throw new ToolBoxUserNotesException("Unsupported ToolBox version");

            string[] mods = response["constants"]["users"].Values<string>().ToArray();

            string[] warnings = response["constants"]["warnings"].Values<string>().ToArray();


            try
            {
                var data = Convert.FromBase64String(response["blob"].Value<string>());

                string uncompressed;
                using (System.IO.MemoryStream compressedStream = new System.IO.MemoryStream(data))
                {
                    compressedStream.ReadByte();
                    compressedStream.ReadByte(); //skips first to bytes to fix zlib block size
                    using (DeflateStream blobStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        using (var decompressedReader = new System.IO.StreamReader(blobStream))
                        {
                            uncompressed = decompressedReader.ReadToEnd();
                        }

                    }
                }

                JObject users = JObject.Parse(uncompressed);

                List<TBUserNote> toReturn = new List<TBUserNote>();
                foreach (KeyValuePair<string, JToken> user in users)
                {
                    var x = user.Value;
                    foreach (JToken note in x["ns"].Children())
                    {

                        TBUserNote uNote = new TBUserNote()
                        {
                            AppliesToUsername = user.Key,
                            SubName = subName,
                            SubmitterIndex = note["m"].Value<int>(),
                            Submitter = mods[note["m"].Value<int>()],
                            NoteTypeIndex = note["w"].Value<int>(),
                            NoteType = warnings[note["w"].Value<int>()],
                            Message = note["n"].Value<string>(),
                            Timestamp = UnixTimeStamp.UnixTimeStampToDateTime(note["t"].Value<long>()),
                            Url = UnsquashLink(subName, note["l"].ValueOrDefault<string>())
                        };
                        toReturn.Add(uNote);
                    }
                }
                return toReturn;
            }
            catch (Exception e)
            {
                throw new ToolBoxUserNotesException("An error occured while processing Usernotes wiki. See inner exception for details", e);
            }
        }

        public static async Task<string[]> GetWarningKeys(IWebAgent webAgent, string subName) {
            var reqResponse = await webAgent.Get(string.Format(ToolBoxUserNotesWiki, subName)).ConfigureAwait(false);
            var response = JObject.Parse(reqResponse["data"]["content_md"].Value<string>());
            int version = response["ver"].Value<int>();
            if(version < 6) throw new ToolBoxUserNotesException("Unsupported ToolBox version");

            string[] warnings = response["constants"]["warnings"].Values<string>().ToArray();

            return warnings;

        }
        public static string UnsquashLink(string subreddit, string permalink)
        {
            var link = "https://reddit.com/r/" + subreddit + "/";
            if (string.IsNullOrEmpty(permalink))
            {
                return link;
            }
            var linkParams = permalink.Split(',');

            if (linkParams[0] == "l")
            {
                link += "comments/" + linkParams[1] + "/";
                if (linkParams.Length > 2)
                    link += "-/" + linkParams[2] + "/";
            }
            else if (linkParams[0] == "m")
            {
                link += "message/messages/" + linkParams[1];
            }
            return link;
        }
    }
}