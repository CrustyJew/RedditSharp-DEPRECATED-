using System;
using System.Linq.Expressions;

namespace RedditSharp.Search
{
    public interface IAdvancedSearchFormatter
    {

        /// <summary>
        /// use an expression to create a search for reddit
        /// </summary>
        /// <param name="search"></param>
        /// <returns>the string representing the search expression in reddits search format</returns>
        /// <remarks>
        /// has to be Expression Func https://michael-mckenna.com/func-t-vs-expression-func-t-in-linq/
        /// </remarks>
        string Format(Expression<Func<AdvancedSearchFilter, bool>> search);
    }
}
