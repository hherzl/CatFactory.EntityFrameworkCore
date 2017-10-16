using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityMapInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetEntityMapInterfaceDefinition(this EfCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            interfaceDefinition.Namespace = project.GetDataLayerMappingNamespace();

            interfaceDefinition.Name = "IEntityMap";

            interfaceDefinition.Methods.Add(new MethodDefinition("void", "Map", new ParameterDefinition("ModelBuilder", "modelBuilder")));

            return interfaceDefinition;
        }
    }
}
