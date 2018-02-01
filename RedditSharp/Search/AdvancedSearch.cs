namespace RedditSharp.Search
{
    public sealed class AdvancedSearchFilter
    {
        public string Author;
        public string Flair;
        public bool IsNsfw;
        public bool IsSelf;
        public string SelfText;
        public string Site;
        public string Subreddit;
        public string Title;
        public string Url;


        private AdvancedSearchFilter() { }
        

    }
}
