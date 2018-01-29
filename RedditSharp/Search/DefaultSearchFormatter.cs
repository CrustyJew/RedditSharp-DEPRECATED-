using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RedditSharp.Search
{
    public class DefaultSearchFormatter : IAdvancedSearchFormatter
    {
        #region Constants
        private const string BOOL_PROPERTY_PREFIX = "Is";
        #endregion Constants


        string IAdvancedSearchFormatter.Format(Expression<Func<AdvancedSearchFilter, bool>> search)
        {
            Expression expression = null;
            Stack<Expression> expressionStack = new Stack<Expression>();
            Stack<FormatInfo> formatInfoStack = new Stack<FormatInfo>();
            expressionStack.Push(search.Body);
            Stack<string> searchStack = new Stack<string>();
            while (expressionStack.Count > 0) {
                expression = expressionStack.Pop();
                switch (expression)
                {
                    case MemberExpression memberExpression:
                        MemberExpressionHelper(memberExpression, searchStack, formatInfoStack);
                        break;
                    case UnaryExpression unaryExpression:
                        UnaryExpressionHelper(unaryExpression, expressionStack, formatInfoStack);
                        break;
                    case BinaryExpression binaryExpression:
                        BinaryExpressionHelper(binaryExpression, expressionStack, formatInfoStack);
                        break;
                    case ConstantExpression constantExpresssion:
                        searchStack.Push(ConstantExpressionHelper(constantExpresssion));
                        break;
                    case MethodCallExpression methodCallExpression:
                        searchStack.Push(MethodCallExpressionHelper(methodCallExpression));
                        break;
                    default:
                        throw new NotImplementedException(expression.ToString());
                }
            }

            Stack<string> compoundSearchStack = new Stack<string>();
            while (formatInfoStack.Count > 0)
            {
                FormatInfo current = formatInfoStack.Pop();
                string[] formatParameters = new string[current.ParameterCount];
                int currentCount = current.ParameterCount;
                while (currentCount > 0)
                {
                    formatParameters[formatParameters.Length - currentCount] = current.IsCompound ? compoundSearchStack.Pop() : searchStack.Pop();
                    currentCount--;
                }

                compoundSearchStack.Push(string.Format(current.Pattern, formatParameters));

            }

            return compoundSearchStack.Pop();
        }

        private string MethodCallExpressionHelper(MethodCallExpression expression)
        {
            var o = InvokeGetExpression(expression);
            return o.ToString();
        }

        private string ConstantExpressionHelper(ConstantExpression constantExpresssion)
        {
            return constantExpresssion.ToString().Replace("\"","");
        }

        private void BinaryExpressionHelper(BinaryExpression expression, Stack<Expression> expressionStack, Stack<FormatInfo> formatInfoStack)
        {
            if(IsAdvancedSearchMemberExpression(expression.Left) && IsAdvancedSearchMemberExpression(expression.Right))
            {
                throw new InvalidOperationException("Cannot filter by comparing to fields.");
            }
            else if(IsAdvancedSearchMemberExpression(expression.Right))
            {
                expressionStack.Push(expression.Left);
                expressionStack.Push(expression.Right);
            }
            else
            {
                expressionStack.Push(expression.Right);
                expressionStack.Push(expression.Left);
            }

            if (expression.NodeType != ExpressionType.Equal)
            {
                formatInfoStack.Push(expression.ToFormatInfo());
            }
        }

     
        private void UnaryExpressionHelper(UnaryExpression expression, Stack<Expression> expressionStack,Stack<FormatInfo> formatInfoStack)
        {
            formatInfoStack.Push(expression.ToFormatInfo());
            expressionStack.Push(expression.Operand);
        }

        private void MemberExpressionHelper(MemberExpression expression, Stack<string> searchStack, Stack<FormatInfo> formatInfoStack)
        {
            MemberInfo member = expression.Member;
            
            if (member.DeclaringType == typeof(AdvancedSearchFilter))
            {
                string result = member.Name.Replace(BOOL_PROPERTY_PREFIX, string.Empty).ToLower();
                formatInfoStack.Push(expression.ToFormatInfo());
                searchStack.Push(result);
                if (expression.Type == typeof(bool))
                {
                    searchStack.Push("1");
                }
            }
            else
            {
                searchStack.Push(InvokeGetExpression(expression).ToString());
            }
        }

        private static object InvokeGetExpression(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        private static bool IsAdvancedSearchMemberExpression(Expression expression)
        {
            MemberExpression memberExpression = expression as MemberExpression;
            return memberExpression?.Member.DeclaringType == typeof(AdvancedSearchFilter);
        }

        internal class FormatInfo
        {
            public string Pattern { get; private set; }
            public int ParameterCount { get; private set; }
            public bool IsCompound { get; private set; }

            public FormatInfo(string pattern, int parameterCount = 0, bool isCompound = false)
            {
                Pattern = pattern;
                ParameterCount = parameterCount;
                IsCompound = isCompound;
            }

            internal static FormatInfo Not = new FormatInfo("NOT(+{0}+)", 1, true);
            internal static FormatInfo NotEqual = Not;
            internal static FormatInfo AndAlso = new FormatInfo("(+{0}+AND+{1}+)", 2, true);
            internal static FormatInfo OrElse = new FormatInfo("(+{0}+OR+{1}+)", 2, true);
            internal static FormatInfo MemberAccess = new FormatInfo("{1}:{0}", 2);
        }
    }
}
