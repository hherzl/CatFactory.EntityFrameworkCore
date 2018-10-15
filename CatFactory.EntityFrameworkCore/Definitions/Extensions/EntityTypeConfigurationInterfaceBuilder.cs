using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityTypeConfigurationInterfaceBuilder
    {
        public static EntityTypeConfigurationInterfaceDefinition GetEntityTypeConfigurationInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new EntityTypeConfigurationInterfaceDefinition
            {
                Namespaces =
                {
                    "Microsoft.EntityFrameworkCore"
                },
                Namespace = project.GetDataLayerConfigurationsNamespace(),
                Name = "IEntityTypeConfiguration",
                Methods =
                {
                    new MethodDefinition("void", "Configure", new ParameterDefinition("ModelBuilder", "modelBuilder"))
                }
            };
    }
}
