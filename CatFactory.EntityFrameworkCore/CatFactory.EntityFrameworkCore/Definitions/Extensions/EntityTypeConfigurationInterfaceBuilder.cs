using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityTypeConfigurationInterfaceBuilder
    {
        public static EntityTypeConfigurationInterfaceDefinition GetEntityTypeConfigurationInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new EntityTypeConfigurationInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            interfaceDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            interfaceDefinition.Name = "IEntityTypeConfiguration";

            interfaceDefinition.Methods.Add(new MethodDefinition("void", "Configure", new ParameterDefinition("ModelBuilder", "modelBuilder")));

            return interfaceDefinition;
        }
    }
}
