namespace ValueConversion.Ef6
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class TargetToMediatorVisitor : ExpressionVisitor
    {
        private readonly MediatorMapper _mapper;

        public TargetToMediatorVisitor(MediatorMapper mapper)
        {
            _mapper = mapper;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var body = Visit(node.Body);
            if (body != node.Body)
            {
                // TODO: what about multiple parameters?
                return Expression.Lambda(body, node.Parameters);
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return base.VisitMemberInit(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            // TODO: checks
            var mediatorType = _mapper.GetMediatorType(node.Type);
            return Expression.New(mediatorType.GetConstructor(Type.EmptyTypes));
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            // Look for a pattern target.Property = (something)exp; -> mediator.Property = exp
            if (node.Member.MemberType == MemberTypes.Property && _mapper.IsTargetType(node.Member.ReflectedType))
            {
                var propertyInfo = (PropertyInfo)node.Member;
                MemberInfo mediatorProperty = _mapper.ConvertToMediator(propertyInfo);

                // Look for a pattern target.Property = (something)exp; -> mediator.Property = exp
                if (node.Expression is UnaryExpression convertExpression
                    && convertExpression.NodeType == ExpressionType.Convert)
                {
                    return Expression.Bind(mediatorProperty, convertExpression.Operand);
                }

                // No cast, so you can use expression directly
                // TODO: Visit expression
                return Expression.Bind(mediatorProperty, node.Expression);
            }

            return base.VisitMemberAssignment(node);
        }
    }
}
