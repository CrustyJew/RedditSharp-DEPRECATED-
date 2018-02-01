namespace RedditSharp
{
    public class SpamFilterSettings
    {
        public SpamFilterStrength LinkPostStrength { get; set; }
        public SpamFilterStrength SelfPostStrength { get; set; }
        public SpamFilterStrength CommentStrength { get; set; }
        /// <summary>
        /// Creates a listing of the default filter lengths (all on high)
        /// </summary>
        public SpamFilterSettings()
        {
            LinkPostStrength = SpamFilterStrength.High;
            SelfPostStrength = SpamFilterStrength.High;
            CommentStrength = SpamFilterStrength.High;
        }
    }
}
