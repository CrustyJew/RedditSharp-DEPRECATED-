[![redditsharp MyGet Build Status](https://www.myget.org/BuildSource/Badge/redditsharp?identifier=0871c1e1-0ab6-489d-9a7f-ce6c2485cfe5)](https://www.myget.org/) [![NuGet version](https://badge.fury.io/nu/redditsharp.svg)](https://badge.fury.io/nu/redditsharp)

**This is a hard fork and IS maintained**.

Due to the project being abandoned and the previous owner's refusal to transfer the repository to someone else to maintain it, I've created this fork to continue on support for the project.

**See CONTRIBUTING.MD for instructions for development**

# RedditSharp

A partial implementation of the [Reddit](http://reddit.com) API. Includes support for many API endpoints, as well as
LINQ-style paging of results.

Reddit rate limits requests by "user". Users can be either the script itself, or a specific user's login info/token. To handle multiple users and their different rate limits, the best solution is to use a "pool" of connections to track it.

`RefreshTokenWebAgentPool` supplies methods to store and retrieve `IWebAgent`s for users by implementing a cache of `RefreshTokenWebAgent`s that automatically get a new access token when the current one expires.

Recently used `IWebAgent`s are stored in an in-memory cache that uses a GUID as a key, and the `IWebAgent` as the object. The default is to have a sliding expiration set so that if a user isn't actively being used, it will expire the `IWebAgent` out of memory, but still have the details it needs to re-create it when it is next needed.

All details to create a new `IWebAgent` that isn't in the cache, but has been used before, are stored in a private array for the pool so it can easily recreate WebAgents as needed.

It can likely be used in a standard .NET application so long as you use a static variable to store the refernce, but below is the recommended method of usage in a .NET CORE application.

```csharp

public class Startup {
	...
	public void ConfigureServices( IServiceCollection services ){
		...
		//Configuration object should be defined and loaded already
		var webAgentPool = new RedditSharp.RefreshTokenWebAgentPool(Configuration["RedditClientID"], Configuration["RedditClientSecret"], Configuration["RedditRedirectURI"])
            {
                DefaultRateLimitMode = RedditSharp.RateLimitMode.Burst,
                DefaultUserAgent = "SnooNotes (by Meepster23)"
            };
		services.AddSingleton(webAgentPool); //Important to add as Singleton so multiple instances aren't created
	}
}

public class SomeController {
	private RedditSharp.RefreshTokenWebAgentPool agentPool;
	
	public void SomeController( RedditSharp.RefreshTokenWebAgentPool webAgentPool){
		agentPool = webAgentPool;
	}
	
	public async Task DoSomething(string username){
		var webAgent =  await agentPool.GetOrCreateWebAgentAsync(user.BannedBy, (uname, uagent, rlimit) =>
            {
				//string refreshToken = await usermanager call to get identity and retrieve refresh token;
				//uname = username for webagent being created
				//uagent = default useragent for webagent being created
				//rlimit = default rate limit mode for webagent being created
				return Task.FromResult<RedditSharp.RefreshTokenPoolEntry>(new RedditSharp.RefreshTokenPoolEntry(uname, refreshToken, rlimit, uagent));
            }
		);
		var reddit = new RedditSharp.Reddit(webAgent, true);
		//use reddit to make calls as user
	}
}
```

A `BotWebAgent` uses a "script" type app and automatically renew access tokens using a username and password

```csharp
var webAgent = new BotWebAgent("BotUsername", "BotPass", "ClientID", "ClientSecret", "RedirectUri");
//This will check if the access token is about to expire before each request and automatically request a new one for you
//"false" means that it will NOT load the logged in user profile so reddit.User will be null
var reddit = new Reddit(webAgent, false);
var subreddit = reddit.GetSubreddit("/r/example");
await subreddit.SubscribeAsync();
await reddit.RSlashAll.New.Take(2).ForEachAsync(page => {
  foreach(var post in page) {
    if (post.Title == "What is my karma?")
    {
        // Note: This is an example. Bots are not permitted to cast votes automatically.
        post.Upvote();
        var comment = post.Comment(string.Format("You have {0} link karma!", post.Author.LinkKarma));
        comment.Distinguish(DistinguishType.Moderator);
    }
  }
});
```

**Important note**: Make sure you use `.Take(int)` when working with pagable content. For example, don't do this:

```csharp
await reddit.RSlashAll.New.ForEachAsync(page => { // BAD
  foreach(var post in page) {
    // ...
  }
});
```

This will cause you to page through everything that has ever been posted on Reddit. Better:

```csharp
await reddit.RSlashAll.New.Take(2).ForEachAsync(page => {
  foreach(var post in page) {
    // ...
  }
});
```


**Using ListingStreams**

Use ListingStreams to infinitely yeild new Things posted to reddit

Example:

```csharp
// get all new comments as they are posted.
var comments = subreddit.Comments.GetListingStream();

await comments.Execute();
foreach (var comment in subreddit.CommentStream)
{
    Console.WriteLine(DateTime.Now + "   New Comment posted to /r/example: " + comment.ShortLink);
}
```

```csharp
// get new modmail
var newModmail = user.ModMail.GetListingStream();
foreach (var message in newModmail)
{
    if (message.FirstMessageName == "")
        message.Reply("Thanks for the message - we will get back to you soon.");
}
```

## Development

RedditSharp is developed with the following workflow:

1. Nothing happens for weeks/months/years
2. Someone needs it to do something it doesn't already do
3. That person implements that something and submits a pull request
4. Repeat

If it doesn't have a feature that you want it to have, add it! The code isn't that scary.
