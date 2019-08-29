using Microsoft.Extensions.Configuration;
using Xunit;

namespace RedditSharpTests
{
    public class AuthenticatedTestsFixture
    {
        public IConfigurationRoot Config { get; private set; }
        public string AccessToken { get; private set; }
        public RedditSharp.BotWebAgent WebAgent { get; set; }
        public string TestUserName { get; private set; }
        public AuthenticatedTestsFixture()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("private.json",true)
            .AddEnvironmentVariables();
            Config = builder.Build();
            WebAgent = new RedditSharp.BotWebAgent(Config["TestUserName"], Config["TestUserPassword"], Config["RedditClientID"], Config["RedditClientSecret"], Config["RedditRedirectURI"]);
            AccessToken = WebAgent.AccessToken;
            TestUserName = Config["TestUserName"];
        }
    }
    [CollectionDefinition("AuthenticatedTests")]
    public class AuthenticatedTestsCollection : ICollectionFixture<AuthenticatedTestsFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
