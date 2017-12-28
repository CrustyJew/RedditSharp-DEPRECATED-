using System;
using System.Linq.Expressions;

namespace RedditSharp.Search
{
    public static class SearchExtensions
    {
        internal static DefaultSearchFormatter.FormatInfo ToFormatInfo(this Expression expression)
        {
            ExpressionType? type = expression?.NodeType;
            switch (type)
            {
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                    return DefaultSearchFormatter.FormatInfo.Not;
                case ExpressionType.Equal:
                    throw new NotImplementedException("Currently not supporting Equal expression.");
                case ExpressionType.AndAlso:
                    return DefaultSearchFormatter.FormatInfo.AndAlso;
                case ExpressionType.MemberAccess:
                    return DefaultSearchFormatter.FormatInfo.MemberAccess;
                case ExpressionType.OrElse:
                    return DefaultSearchFormatter.FormatInfo.OrElse;
            }
            throw new NotImplementedException($"{type.ToString()} is not implemented.");
        }

    }
}
