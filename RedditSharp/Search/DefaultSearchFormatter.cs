using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Search
{
    public class DefaultSearchFormatter : ISearchFormatter
    {
        string ISearchFormatter.Format(Expression<Func<AdvancedSearchFilter, bool>> search)
        {
            Expression expression = null;
            Stack<Expression> expressionStack = new Stack<Expression>();
            expressionStack.Push(search.Body);
            Stack<string> searchStack = new Stack<string>();
            while (expressionStack.Count > 0) {
                expression = expressionStack.Pop();
                switch (expression)
                {
                    case MemberExpression memberExpression:
                        searchStack.Push(MemberExpressionHelper(memberExpression));
                        break;
                    case UnaryExpression unaryExpression:
                        searchStack.Push(UnaryExpressionHelper(unaryExpression,expressionStack));
                        break;
                    case BinaryExpression binaryExpression:
                        BinaryExpressionHelper(binaryExpression, expressionStack, searchStack);
                        break;
                    case ConstantExpression constantExpresssion:
                        searchStack.Push(ConstantExpressionHelper(constantExpresssion));
                        break;
                    default:
                        throw new NotImplementedException(expression.ToString());
                }
            }

            string searchQuery = searchStack.Pop();
            while(searchStack.Count > 0)
            {
                searchQuery = string.Format(searchStack.Pop(), searchQuery);
            }
            return searchQuery;
        }

        private string ConstantExpressionHelper(ConstantExpression constantExpresssion)
        {
            return constantExpresssion.ToString().Replace("\"","");
        }

        private void BinaryExpressionHelper(BinaryExpression expression, Stack<Expression> expressionStack, Stack<string> searchStack)
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
                searchStack.Push(expression.ToOperator());
                //searchStack.Push("NOT(+{0}+)");
            }
            

        }

     
        private string UnaryExpressionHelper(UnaryExpression expression, Stack<Expression> expressionStack)
        {
            string expressionOperator = expression.ToOperator();
            expressionStack.Push(expression.Operand);
            return expressionOperator;
        }

        private string MemberExpressionHelper(MemberExpression expression)
        {
            MemberInfo member = expression.Member;
            string result = string.Empty;

            if (member.DeclaringType == typeof(AdvancedSearchFilter))
            {
                result = member.Name.Replace(BOOL_PROPERTY_PREFIX, string.Empty).ToLower();
                if (expression.Type == typeof(bool))
                {
                    result = result + ":1";
                }
                else
                {
                    result = result + ":{0}";
                }

            }
            return result;
        }


        



        private const string BOOL_PROPERTY_PREFIX = "Is";


        private static readonly List<ExpressionType> conditionalTypes = new List<ExpressionType>()
        {
            ExpressionType.AndAlso,
            ExpressionType.And,
            ExpressionType.OrElse,
            ExpressionType.Or
        };

        private static readonly List<ExpressionType> evaluateExpressions = new List<ExpressionType>()
        {
            ExpressionType.Add,
            ExpressionType.Subtract,
            ExpressionType.Multiply,
            ExpressionType.Divide,
            ExpressionType.Coalesce,
            ExpressionType.Conditional
        };

        private static bool IsAdvancedSearchMemberExpression(Expression expression)
        {
            MemberExpression memberExpression = expression as MemberExpression;
            return memberExpression?.Member.DeclaringType == typeof(AdvancedSearchFilter);
        }
    }

    public static class Extensions
    {
        public static string ToOperator(this Expression expression)
        {
            ExpressionType? type = expression?.NodeType;
            string result = string.Empty;
            switch (type)
            {
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                    result = "NOT(+{0}+)";
                    break;
                case ExpressionType.Equal:
                    result = ":";
                    break;
                case ExpressionType.AndAlso:
                    result = "(+{0}+AND)";
                    break;
            }
            return result;
        }

    }
}
