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
    public partial class MediatorTypeBuilder
    {
        private const string _assemblyName = "ValueConversion.Ef6.MediatorAssembly";

        public MediatorMapper CreateMediatorTypes(TargetTypeGraph graph)
        {
            var assemblyName = new AssemblyName(_assemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var targetToMediators = new Dictionary<Type, TypeBuilder>();
            foreach (var node in graph.Nodes)
            {
                var targetType = node.Type;
                var mediatorTypeName = targetType.Namespace + ".«" + targetType.Name + "»";

                // TODO: Does it need to be public?
                var typeBuilder = moduleBuilder.DefineType(mediatorTypeName, TypeAttributes.Public);
                typeBuilder.DefineDefaultConstructor();
                foreach (var columnMember in node.ColumnMembers)
                {
                    typeBuilder.DefineMediatorColumnMember(columnMember);
                }

                targetToMediators.Add(targetType, typeBuilder);
            }

            foreach (var edge in graph.Edges)
            {
                var fromNodeTypeBuilder = targetToMediators[edge.From.Type];
                fromNodeTypeBuilder.DefineMediatorMember(edge.Member, targetToMediators);
            }

            var mediatorMaps = targetToMediators.Select(targetToMediator =>
            {
                var targetType = targetToMediator.Key;
                var mediatorType = targetToMediator.Value.CreateType();

                return new MediatorTypeMap(targetType, mediatorType);
            });

            return new MediatorMapper(mediatorMaps);
        }
    }
}
