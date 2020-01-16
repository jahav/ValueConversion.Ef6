namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// A visitor that goes through a query expression and finds all types used in the query.
    /// </summary>
    public class MaterializedTypesVisitor : ExpressionVisitor
    {
        private readonly HashSet<Type> _materializedTypes = new HashSet<Type>();
        private readonly CastMap _castMap = new CastMap();

        /// <summary>
        /// Gets the list of types used in the materialization.
        /// </summary>
        public IReadOnlyCollection<Type> MaterializedTypes => _materializedTypes;

        protected override Expression VisitNew(NewExpression node)
        {
            _materializedTypes.Add(node.Type);
            return base.VisitNew(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.ConvertChecked || node.NodeType == ExpressionType.Convert)
            {
                _castMap.AddFromTo(node.Operand.Type, node.Type);
            }

            return base.VisitUnary(node);
        }
    }
}
