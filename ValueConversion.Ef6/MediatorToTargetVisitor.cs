namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class MediatorToTargetVisitor : ExpressionVisitor
    {
        private readonly MediatorMapper _mapper;

        private readonly Stack<MemberInfo> _memberStack = new Stack<MemberInfo>();

        private ParameterExpression? _root = null;

        public MediatorToTargetVisitor(MediatorMapper mapper)
        {
            _mapper = mapper;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (_root == null)
            {
                var mediatorType = _mapper.GetMediatorType(node.Body.Type);
                _root = Expression.Parameter(mediatorType, "mediator");
            }

            return Expression.Lambda(Visit(node.Body), _root);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            if (node.Member is PropertyInfo property)
            {
                if (_mapper.IsTargetType(property.PropertyType))
                {
                    _memberStack.Push(node.Member);
                    var expr = base.VisitMemberAssignment(node);
                    _memberStack.Pop();
                    return expr;
                }
                else
                {
                    Expression n = _root;
                    foreach (var member in _memberStack)
                    {
                        n = Expression.PropertyOrField(n, member.Name);
                    }

                    n = Expression.PropertyOrField(n, node.Member.Name);

                    return Expression.Bind(node.Member, n);
                }
            }

            throw new NotSupportedException($"Member {node.Member} is not supported.");
        }
    }
}