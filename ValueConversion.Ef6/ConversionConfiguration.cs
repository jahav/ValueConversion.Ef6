namespace ValueConversion.Ef6
{
    using System;
    using System.Reflection;

    public class ConversionConfiguration
    {
        /// <summary>
        /// Gets or sets a delegate that determines if a CLR type can be used as an alias of a DB column type.
        /// This is heavily dependent on EF6, see allowed casts CLR types in <c>ExpressionConverter.GetCastTargetType</c>.
        /// For primitive types (that is not every type, e.g. ), see EF6 source <c>ClrProviderManifest.TryGetPrimitiveTypeKind</c> for a list of allowed types in EF6 as a column.
        /// </summary>
        public Func<Type, bool> IsAllowedForColumn { get; set; } = x => true;

        /// <summary>
        /// Gets or sets a delegate that determines if a mediator should contain a propery with same name as the target.
        /// </summary>
        public Func<PropertyInfo, bool> ShouldMediateTargetProperty { get; set; } = x => x.SetMethod != null && x.GetMethod != null;
    }
}
