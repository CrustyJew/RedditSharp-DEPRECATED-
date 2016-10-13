[![redditsharp MyGet Build Status](https://www.myget.org/BuildSource/Badge/redditsharp?identifier=038d3763-6401-4c84-b579-f5134e1c8efd)](https://www.myget.org/) [![NuGet version](https://badge.fury.io/nu/redditsharp.svg)](https://badge.fury.io/nu/redditsharp)

**This is a hard fork and IS maintained**. 

Due to the project being abandoned and the previous owner's refusal to transfer the repository to someone else to maintain it, I've created this fork to continue on support for the project.

# RedditSharp

A partial implementation of the [Reddit](http://reddit.com) API. Includes support for many API endpoints, as well as
LINQ-style paging of results.

```csharp
var webAgent = new BotWebAgent("BotUsername", "BotPass", "ClientID", "ClientSecret", "RedirectUri");
//This will check if the access token is about to expire before each request and automatically request a new one for you
//"false" means that it will NOT load the logged in user profile so reddit.User will be null
var reddit = new Reddit(webAgent, false);
var subreddit = reddit.GetSubreddit("/r/example");
subreddit.Subscribe();
foreach (var post in subreddit.New.Take(25))
{
    if (post.Title == "What is my karma?")
    {
        // Note: This is an example. Bots are not permitted to cast votes automatically.
        post.Upvote();
        var comment = post.Comment(string.Format("You have {0} link karma!", post.Author.LinkKarma));
        comment.Distinguish(DistinguishType.Moderator);
    }
}
```

**Important note**: Make sure you use `.Take(int)` when working with pagable content. For example, don't do this:

```csharp
var all = reddit.RSlashAll;
foreach (var post in all.New) // BAD
{
    // ...
}
```

This will cause you to page through everything that has ever been posted on Reddit. Better:

```csharp
var all = reddit.RSlashAll;
foreach (var post in all.New.Take(25))
{
    // ...
}
```


**Using ListingStreams**

Use ListingStreams to infinitely yeild new Things posted to reddit

Example:

```csharp
// get all new comments as they are posted.
foreach (var comment in subreddit.CommentStream)
{
    Console.WriteLine(DateTime.Now + "   New Comment posted to /r/example: " + comment.ShortLink);
}
```

you can call .GetListingStream() on any Listing<Thing>

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
