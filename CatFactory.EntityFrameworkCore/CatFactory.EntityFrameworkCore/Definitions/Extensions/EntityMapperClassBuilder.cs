using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityMapperClassBuilder
    {
        public static EntityMapperClassDefinition GetEntityMapperClassDefinition(this EntityFrameworkCoreProject project)
            => new EntityMapperClassDefinition
            {
                Namespaces =
                {
                    "System.Collections.Generic",
                    "Microsoft.EntityFrameworkCore"
                },
                Namespace = project.GetDataLayerConfigurationsNamespace(),
                Name = "EntityMapper",
                Implements =
                {
                    "IEntityMapper"
                },
                Constructors =
                {
                    new ClassConstructorDefinition()
                },
                Properties =
                {
                    new PropertyDefinition("IEnumerable<IEntityTypeConfiguration>", "Configurations")
                },
                Methods =
                {
                    new MethodDefinition("void", "ConfigureEntities", new ParameterDefinition("ModelBuilder", "modelBuilder"))
                    {
                        Lines =
                        {
                            new CodeLine("foreach (var item in Configurations)"),
                            new CodeLine("{"),
                            new CodeLine(1, "item.Configure(modelBuilder);"),
                            new CodeLine("}")
                        }
                    }
                }
            };
    }
}
