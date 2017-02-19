#pragma warning disable 1591
using System;

namespace RedditSharp
{
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
#pragma warning restore 1591