namespace RedditSharp
{
    /// <summary>
    /// Response for a captcha challenge.
    /// </summary>
    public class CaptchaResponse
    {
        /// <summary>
        /// Captcha answer.
        /// </summary>
        public readonly string Answer;

        /// <summary>
        /// Set to true to cancel.
        /// </summary>
        public bool Cancel { get { return string.IsNullOrEmpty(Answer); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="answer"></param>
        public CaptchaResponse(string answer = null)
        {
            Answer = answer;
        }
    }
}
