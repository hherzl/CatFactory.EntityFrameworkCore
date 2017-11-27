using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityTypeConfigurationInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetEntityTypeConfigurationInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            interfaceDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            interfaceDefinition.Name = "IEntityTypeConfiguration";

            interfaceDefinition.Methods.Add(new MethodDefinition("void", "Configure", new ParameterDefinition("ModelBuilder", "modelBuilder")));

            return interfaceDefinition;
        }
    }
}
