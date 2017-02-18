using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Xunit;

[assembly: UserSecretsId("aspnet-RedditSharpTests-20170213104014")]
namespace RedditSharpTests
{
    public class AuthenticatedTestsFixture
    {
        public IConfigurationRoot Config { get; private set; }
        public RedditSharp.AuthProvider AuthProvider { get; private set; }
        public string AccessToken { get; private set; }
        public string TestUserName { get; private set; }
        public AuthenticatedTestsFixture()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets<AuthenticatedTestsFixture>();
            Config = builder.Build();

            AuthProvider = new RedditSharp.AuthProvider(Config["RedditClientID"], Config["RedditClientSecret"], Config["RedditRedirectURI"]);
            AccessToken = AuthProvider.GetOAuthTokenAsync(Config["TestUserRefreshToken"], isRefresh: true).Result;
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
