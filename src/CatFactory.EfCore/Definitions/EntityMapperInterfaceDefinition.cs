using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityMapperInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetEntityMapperInterfaceDefinition(this EfCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("System.Collections.Generic");
            interfaceDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            interfaceDefinition.Namespace = project.GetDataLayerMappingNamespace();

            interfaceDefinition.Name = "IEntityMapper";

            interfaceDefinition.Properties.Add(new PropertyDefinition("IEnumerable<IEntityMap>", "Mappings") { IsReadOnly = true });

            interfaceDefinition.Methods.Add(new MethodDefinition("void", "MapEntities", new ParameterDefinition("ModelBuilder", "modelBuilder")));

            return interfaceDefinition;
        }
    }
}
