using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityMapperInterfaceBuilder
    {
        public static EntityMapperInterfaceDefinition GetEntityMapperInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new EntityMapperInterfaceDefinition
            {
                Namespaces =
                {
                    "System.Collections.Generic",
                    "Microsoft.EntityFrameworkCore"
                },
                Namespace = project.GetDataLayerConfigurationsNamespace(),
                Name = "IEntityMapper",
                Properties =
                {
                    new PropertyDefinition("IEnumerable<IEntityTypeConfiguration>", "Configurations")
                    {
                        IsReadOnly = true
                    }
                },
                Methods =
                {
                    new MethodDefinition("void", "ConfigureEntities", new ParameterDefinition("ModelBuilder", "modelBuilder"))
                }
            };
    }
}
