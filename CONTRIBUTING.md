# Development

Use a private.json file to pull in config for tests. The required format is as follows

```json
{
  "RedditClientID": "",
  "RedditClientSecret": "",
  "RedditRedirectURI": "",
  "TestUserName": "",
  "TestUserPassword":"",
  "TestSubreddit": "RedditSharpDev"
}
```

To get a ClientID, Secret, and RedirectURI, as your test account, log in to Reddit. 
* Click Preferences. 
* Go to the "apps" tab
* Create a new app with type "script". 

For the RedirectURI you can enter anything you want, just make sure it matches in your config.

The string under your app's name and "personal use script" will be your RedditClientID. If you click edit, you will see the secret listed.

You can use your own subreddit for testing, but you can also message [/u/meepster23](https://reddit.com/u/meepster23) for moderator
access to [/r/RedditSharpDev](https://reddit.com/r/RedditSharpDev).

Please make sure to follow the style of the repo. The easiest way to do that is to install the Visual Studio extension called "Rebracer".
The config file included will automatically adjust your Visual Studio settings for proper indents etc.