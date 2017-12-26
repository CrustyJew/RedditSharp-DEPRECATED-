using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RedditSharp.Search
{
    public interface ISearchFormatter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /// <remarks>
        /// has to be Expression Func https://michael-mckenna.com/func-t-vs-expression-func-t-in-linq/
        /// </remarks>
        string Format(Expression<Func<AdvancedSearchFilter, bool>> search);
    }
}
