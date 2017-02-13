# Development

Use a private config or Secret Manager to pull in config for tests. The required format is as follows

```json
{
  "RedditClientID": "",
  "RedditClientSecret": "",
  "RedditRedirectURI": "",
  "TestUserRefreshToken": "",
  "TestUserName": "",
  "TestSubreddit": "RedditSharpDev"
}
```

You can use your own subreddit for testing, but you can also message [/u/meepster23](https://reddit.com/u/meepster23) for moderator
access to [/r/RedditSharpDev](https://reddit.com/r/RedditSharpDev).

Please make sure to follow the style of the repo. The easiest way to do that is to install the Visual Studio extension called "Rebracer".
The config file included will automatically adjust your Visual Studio settings for proper indents etc.