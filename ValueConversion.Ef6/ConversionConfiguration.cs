namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public partial class ConversionConfiguration
    {
        private readonly List<ValueConverter> _converters = new List<ValueConverter>();

        public ConversionConfiguration()
        {
            IsAllowedForColumn = x => TypeHelper.MemberTypeSupportedByEf(x) || _converters.Any(y => y.ModelClrType == x);
        }

        /// <summary>
        /// Gets or sets a delegate that determines if a CLR type in target can be used as an alias of a DB column type (e.g. if it is a EF primitive type or has a converter).
        /// This is heavily dependent on EF6, see allowed casts CLR types in <c>ExpressionConverter.GetCastTargetType</c>.
        /// For primitive types (that is not every type, e.g. ), see EF6 source <c>ClrProviderManifest.TryGetPrimitiveTypeKind</c> for a list of allowed types in EF6 as a column.
        /// </summary>
        /// <remarks>
        /// This has a preference, if this says it can be materialized, it is put into the mediator type.</remarks>
        public Func<Type, bool> IsAllowedForColumn { get; set; }

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

        public IReadOnlyCollection<ValueConverter> Converters => _converters;

        public void AddConvertor<TModel, TProvider>(ValueConverter<TModel, TProvider> valueConverter)
        {
            if (!TypeHelper.MemberTypeSupportedByEf(typeof(TProvider)))
            {
                throw new NotSupportedException($"A provider type {typeof(TProvider)} is not supported. The provider type can be only a type supported by EF.");
            }

            if (typeof(TModel) == typeof(TProvider))
            {
                throw new ArgumentException($"Converter converts between same type {typeof(TModel)}.");
            }

            _converters.Add(valueConverter);
        }
    }
}
