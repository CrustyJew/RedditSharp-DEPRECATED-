using System;
using System.Collections.Generic;
using System.Linq;
using RedditSharp;
using System.Security.Authentication;
using RedditSharp.Things;

namespace TestRedditSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Reddit reddit = null;
            var authenticated = false;
            while (!authenticated)
            {
                Console.Write("OAuth? (y/n) [n]: ");
                var oaChoice = Console.ReadLine();
                if (!string.IsNullOrEmpty(oaChoice) && oaChoice.ToLower()[0] == 'y')
                {
                    Console.Write("OAuth token: ");
                    var token = Console.ReadLine();
                    reddit = new Reddit(token);
                    reddit.InitOrUpdateUser();
                    authenticated = reddit.User != null;
                    if (!authenticated)
                        Console.WriteLine("Invalid token");
                }
                else
                {
                    Console.Write("Username: ");
                    var username = Console.ReadLine();
                    Console.Write("Password: ");
                    var password = ReadPassword();
                    try
                    {
                        Console.WriteLine("Logging in...");
                        reddit = new Reddit(username, password);
                        authenticated = reddit.User != null;
                    }
                    catch (AuthenticationException)
                    {
                        Console.WriteLine("Incorrect login.");
                        authenticated = false;
                    }
                }
            }
            /*Console.Write("Create post? (y/n) [n]: ");
            var choice = Console.ReadLine();
            if (!string.IsNullOrEmpty(choice) && choice.ToLower()[0] == 'y')
            {
                Console.Write("Type a subreddit name: ");
                var subname = Console.ReadLine();
                var sub = reddit.GetSubreddit(subname);
                Console.WriteLine("Making test post");
                var post = sub.SubmitTextPost("RedditSharp test", "This is a test post sent from RedditSharp");
                Console.WriteLine("Submitted: {0}", post.Url);
            }
            else
            {
                Console.Write("Type a subreddit name: ");
                var subname = Console.ReadLine();
                var sub = reddit.GetSubreddit(subname);
                foreach (var post in sub.GetTop(FromTime.Week).Take(10))
                    Console.WriteLine("\"{0}\" by {1}", post.Title, post.Author);
            }
			Console.Write("Check inbox for unread private messages? (y/n) [n]: ");
	        var choicePM = Console.ReadLine();
	        if (!string.IsNullOrEmpty(choicePM) && choicePM.ToLower()[0] == 'y')
	        {
		        if (reddit.User.HasMail)
		        {
			        int i = 1;
			        foreach (PrivateMessage message in reddit.User.UnreadMessages.OfType<PrivateMessage>())
			        {
				        Console.WriteLine("({0}) Sender: {1}", i, message.Author);
				        Console.WriteLine("Subject: {0}", message.Subject);
				        Console.WriteLine("Message:");
				        Console.WriteLine("{0}", message.Body);
				        Console.WriteLine("---- ---- ---- ---- ---- ---- ---- ----");
				        i++;
			        }
					Console.WriteLine("End of unread private messages");
					Console.WriteLine("");

				}
		        else
		        {
			        Console.WriteLine("No unread private messages found");
			        Console.WriteLine("");
		        }
	        }

			Console.Write("Check for replies to comments? (y/n) [n]: ");
	        var choiceCR = Console.ReadLine();
	        if (!string.IsNullOrEmpty(choiceCR) && choiceCR.ToLower()[0] == 'y')
	        {
		        if (reddit.User.HasMail)
		        {
			        int i = 1;
			        foreach (Comment commentReply in reddit.User.UnreadMessages.OfType<Comment>())
			        {
				        Console.WriteLine("({0}) Sender: {1}", i, commentReply.Author);
						Console.WriteLine("Thread Title: {0}", commentReply.LinkTitle);
						Console.WriteLine("Message:");
						Console.WriteLine("{0}", commentReply.Body);
						Console.WriteLine("---- ---- ---- ---- ---- ---- ---- ----");
						i++;
					}
					Console.WriteLine("End of unread replies to comments");
					Console.WriteLine("");
		        }
		        else
		        {
					Console.WriteLine("No unread replies to comments found");
					Console.WriteLine("");
				}
	        }
			*/
            Comment comment = (Comment)reddit.GetThingByFullname("t1_ciif2g7");
            Post post = (Post)reddit.GetThingByFullname("t3_298g7j");
            PrivateMessage pm = (PrivateMessage)reddit.GetThingByFullname("t4_20oi3a"); // Use your own PM here, as you don't have permission to view this one
            Console.WriteLine(comment.Body);
            Console.WriteLine(post.Title);
            Console.WriteLine(pm.Body);
            Console.WriteLine(post.Comment("test").FullName);
            Console.ReadKey(true);
        }

        public static string ReadPassword()
        {
            var passbits = new Stack<string>();
            //keep reading
            for (ConsoleKeyInfo cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true))
            {
                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (passbits.Count() > 0)
                    {
                        //rollback the cursor and write a space so it looks backspaced to the user
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        passbits.Pop();
                    }
                }
                else
                {
                    Console.Write("*");
                    passbits.Push(cki.KeyChar.ToString());
                }
            }
            string[] pass = passbits.ToArray();
            Array.Reverse(pass);
            Console.Write(Environment.NewLine);
            return string.Join(string.Empty, pass);
        }
    }
}
