namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// A builder that creates new anonymous types based on domain entities.
    /// The mediator type will have same properties as EF entity, but their type
    /// will be a primitive type (according to Edm), not domain type (e.g. <c>string</c>
    /// instead of <c>SerialNumber</c>).
    /// </summary>
    public class MediatorTypeBuilder
    {
        private const string _assemblyName = "ValueConversion.Ef6.MediatorAssembly";
        private readonly ConversionConfiguration _configuration;

        /// <summary>
        /// List of custom typef for member, anything not found here is just passed through without type change.
        /// </summary>
        private readonly IReadOnlyDictionary<Type, Type> _typeConverters;

        public MediatorTypeBuilder(IReadOnlyDictionary<Type, Type> typeConverters, ConversionConfiguration configuration)
        {
            _configuration = configuration;
            _typeConverters = typeConverters.ToDictionary();
        }

        public Type CreateMediatorType(Type targetType)
        {
            // DO NOT MODIFY, UNLESS YOU KNOW WHAT YOU ARE DOING.
            // Based on a TypeBuilder MSDN example, but I am not good at IL.

            // TODO: Shouldn't create assembly and module for each type, also check that type is not created twice
            var assemblyName = new AssemblyName(_assemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var uniqueName = targetType.FullName + "Proxy";
            var typeBuilder = moduleBuilder.DefineType(uniqueName, TypeAttributes.Public);

            DefineDefaultConstructor(typeBuilder);

            var mediatedProperties = targetType.GetProperties().Where(propInfo => _configuration.ShouldMediateTargetProperty(propInfo));
            foreach (var mediatedProperty in mediatedProperties)
            {
                DefineProperty(typeBuilder, mediatedProperty);
            }

            // TODO: Fields
            return typeBuilder.CreateType();
        }

        private void DefineProperty(TypeBuilder typeBuilder, PropertyInfo propInfo)
        {
            var propertyName = propInfo.Name;
            var propertyType = propInfo.PropertyType;
            var fieldName = "_internal_" + propertyName;

            FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);

            // TODO: not sure what does PropertyAttributes.HasDefault actually do
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, Type.EmptyTypes);

            // The property "set" and property "get" methods require a special
            // set of attributes.
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder getter = typeBuilder.DefineMethod(
                "get_" + propertyName,
                getSetAttr,
                propertyType,
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
            MethodBuilder setter = typeBuilder.DefineMethod(
                "set_" + propertyName,
                getSetAttr,
                null,
                new Type[] { propertyType });

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

        private void DefineDefaultConstructor(TypeBuilder typeBuilder)
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
    }
}
