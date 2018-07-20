using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class DatabaseEntityMapperClassBuilder
    {
        public static DatabaseEntityMapperClassDefinition GetDatabaseEntityMapperClassDefinition(this EntityFrameworkCoreProject project)
        {
            var classDefinition = new DatabaseEntityMapperClassDefinition
            {
                Namespaces =
                {
                    "System.Collections.Generic"
                },
                Namespace = project.GetDataLayerConfigurationsNamespace(),
                Name = project.Database.GetDbEntityMapperName(),
                BaseClass = "EntityMapper"
            };

            var lines = new List<ILine>
            {
                new CodeLine("Configurations = new List<IEntityTypeConfiguration>()"),
                new CodeLine("{")
            };

            for (var i = 0; i < project.Database.Tables.Count; i++)
            {
                var table = project.Database.Tables[i];

                lines.Add(new CodeLine(1, "new {0}(){1}", table.GetEntityTypeConfigurationName(), i == project.Database.Tables.Count - 1 ? string.Empty : ","));
            }

            lines.Add(new CodeLine("};"));

            classDefinition.Constructors.Add(new ClassConstructorDefinition { Lines = lines });

            return classDefinition;
        }
    }
}
