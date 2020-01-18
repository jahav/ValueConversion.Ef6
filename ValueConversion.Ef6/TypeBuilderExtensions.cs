namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class TypeBuilderExtensions
    {
        internal static void DefineDefaultConstructor(this TypeBuilder typeBuilder)
        {
            ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            ILGenerator ctorIL = ctor.GetILGenerator();

            // For a constructor, argument zero is a reference to the new
            // instance. Push it on the stack before calling the base
            // class constructor. Specify the default constructor of the
            // base class (System.Object) by passing an empty array of
            // types (Type.EmptyTypes) to GetConstructor.
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ret);
        }

        internal static void DefineMediatorColumnMember(this TypeBuilder typeBuilder, MemberInfo columnMember)
        {
            typeBuilder.DefineMediatorMember(columnMember, new Dictionary<Type, TypeBuilder>());
        }

        internal static void DefineMediatorMember(this TypeBuilder typeBuilder, MemberInfo targetMember, IReadOnlyDictionary<Type, TypeBuilder> targetToMediator)
        {
            if (targetMember is PropertyInfo property)
            {
                var targetPropertyType = property.PropertyType;
                Type mediatorPropertyType;
                if (targetToMediator.TryGetValue(targetPropertyType, out TypeBuilder mediatorType))
                {
                    mediatorPropertyType = mediatorType;
                }
                else
                {
                    mediatorPropertyType = targetPropertyType;
                }

                DefineMediatorProperty(typeBuilder, property, mediatorPropertyType);
            }
            else
            {
                throw new NotSupportedException($"Unable to create a mediator member {targetMember.DeclaringType}.{targetMember.Name} with member type {targetMember.MemberType}.");
            }
        }

        private static void DefineMediatorProperty(TypeBuilder mediatorTypeBuilder, PropertyInfo targetProperty, Type mediatorPropertyType)
        {
            var propertyName = targetProperty.Name;
            var fieldName = "_internal_" + propertyName;

            FieldBuilder fieldBuilder = mediatorTypeBuilder.DefineField(fieldName, mediatorPropertyType, FieldAttributes.Private);

            // TODO: not sure what does PropertyAttributes.HasDefault actually do
            PropertyBuilder propertyBuilder = mediatorTypeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, mediatorPropertyType, Type.EmptyTypes);

            // The property "set" and property "get" methods require a special
            // set of attributes.
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder getter = mediatorTypeBuilder.DefineMethod(
                "get_" + propertyName,
                getSetAttr,
                mediatorPropertyType,
                Type.EmptyTypes);

            ILGenerator getterIL = getter.GetILGenerator();

            // For an instance property, argument zero is the instance. Load the
            // instance, then load the private field and return, leaving the
            // field value on the stack.
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // Define the "set" accessor method for Number, which has no return
            // type and takes one argument of type int (Int32).
            MethodBuilder setter = mediatorTypeBuilder.DefineMethod(
                "set_" + propertyName,
                getSetAttr,
                null,
                new Type[] { mediatorPropertyType });

            ILGenerator setterIL = setter.GetILGenerator();

            // Load the instance and then the numeric argument, then store the
            // argument in the field.
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            // Last, map the "get" and "set" accessor methods to the
            // PropertyBuilder. The property is now complete.
            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);
        }
    }
}
