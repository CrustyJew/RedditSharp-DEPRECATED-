using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace RedditSharp
{
    /// <summary>
    /// Builds form post data.
    /// </summary>
    public class MultipartFormBuilder
    {
        /// <summary>
        /// Web request.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        private string Boundary { get; set; }
        private MemoryStream Buffer { get; set; }
        private TextWriter TextBuffer { get; set; }

        private MultipartFormDataContent Content { get; set; }

        public MultipartFormBuilder(HttpRequestMessage request)
        {
            // TODO: See about regenerating the boundary when needed
            Request = request;
            var random = new Random();
            Boundary = "----------" + CreateRandomBoundary();
            Content = new MultipartFormDataContent(Boundary);
            Buffer = new MemoryStream();
            TextBuffer = new StreamWriter(Buffer);
        }
        /// <summary>
        /// Creates a random boundary that we use for buffers
        /// </summary>
        /// <returns></returns>
        private string CreateRandomBoundary()
        {
            // TODO: There's probably a better way to go about this
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string value = "";
            var random = new Random();
            for (int i = 0; i < 10; i++)
                value += characters[random.Next(characters.Length)];
            return value;
        }

        /// <summary>
        /// Add a dynamic object to this form.
        /// </summary>
        /// <param name="data"></param>
        public void AddDynamic(object data)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                AddString(property.Name, entry);
            }
        }

        /// <summary>
        /// Add a string value to this form.
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="value">value</param>
        public void AddString(string name, string value)
        {
            TextBuffer.Write("{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                "--" + Boundary, name, value);
            TextBuffer.Flush();
        }

        /// <summary>
        /// Add a file to this form
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filename"></param>
        /// <param name="value"></param>
        /// <param name="contentType"></param>
        public void AddFile(string name, string filename, byte[] value, string contentType)
        {
            TextBuffer.Write("{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                "--" + Boundary, name, filename, contentType);
            TextBuffer.Flush();
            Buffer.Write(value, 0, value.Length);
            Buffer.Flush();
            TextBuffer.Write("\r\n");
            TextBuffer.Flush();
        }

        /// <summary>
        /// Finish this form.
        /// </summary>
        public void Finish()
        {
            TextBuffer.Write("--" + Boundary + "--");
            TextBuffer.Flush();
            Buffer.Seek(0, SeekOrigin.Begin);
            var stream = new StreamContent(Buffer);
            Content.Add(stream);
        }
    }
}
