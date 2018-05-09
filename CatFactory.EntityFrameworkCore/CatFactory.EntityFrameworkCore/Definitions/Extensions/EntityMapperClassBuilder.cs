using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityMapperClassBuilder
    {
        public static EntityMapperClassDefinition GetEntityMapperClassDefinition(this EntityFrameworkCoreProject project)
        {
            var classDefinition = new EntityMapperClassDefinition();

            classDefinition.Namespaces.Add("System.Collections.Generic");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            classDefinition.Name = "EntityMapper";

            classDefinition.Implements.Add("IEntityMapper");

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            classDefinition.Properties.Add(new PropertyDefinition("IEnumerable<IEntityTypeConfiguration>", "Configurations"));

            classDefinition.Methods.Add(new MethodDefinition("void", "ConfigureEntities", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = new List<ILine>
                {
                    new CodeLine("foreach (var item in Configurations)"),
                    new CodeLine("{"),
                    new CodeLine(1, "item.Configure(modelBuilder);"),
                    new CodeLine("}")
                }
            });

            return classDefinition;
        }
    }
}
