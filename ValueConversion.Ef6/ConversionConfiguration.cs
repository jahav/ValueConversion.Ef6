namespace ValueConversion.Ef6
{
    using System;
    using System.Reflection;

    public partial class ConversionConfiguration
    {
        /// <summary>
        /// Gets or sets a delegate that determines if a CLR type can be used as an alias of a DB column type.
        /// This is heavily dependent on EF6, see allowed casts CLR types in <c>ExpressionConverter.GetCastTargetType</c>.
        /// For primitive types (that is not every type, e.g. ), see EF6 source <c>ClrProviderManifest.TryGetPrimitiveTypeKind</c> for a list of allowed types in EF6 as a column.
        /// </summary>
        /// <remarks>
        /// This has a preference, if this says it can be materialized, it is put into the mediator type.</remarks>
        public Func<Type, bool> IsAllowedForColumn { get; set; } = TypeHelper.MemberTypeSupportedByEf;

        /// <summary>
        /// Gets or sets a delegate that determines if a mediator should contain a propery with same name as the target.
        /// </summary>
        public Func<PropertyInfo, bool> ShouldMediateTargetProperty { get; set; } =
            x => x.SetMethod != null && x.SetMethod.IsPublic && x.GetMethod != null && x.GetMethod.IsPublic;

        /// <summary>
        /// Gets or sets a recursion limit that detects endless cycles, e.g. when looking for a connected graph to translate.
        /// First level is 0, once level is greater that MaxRecursion, throw.
        /// </summary>
        public int MaxRecursion { get; set; } = 20;
    }
}
