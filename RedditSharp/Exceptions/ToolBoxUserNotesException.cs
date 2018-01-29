using System;

namespace RedditSharp
{
    [Serializable]
    class ToolBoxUserNotesException : Exception
    {
        public ToolBoxUserNotesException()
        {
        }

        public ToolBoxUserNotesException(string message)
            : base(message)
        {
        }

        public ToolBoxUserNotesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}