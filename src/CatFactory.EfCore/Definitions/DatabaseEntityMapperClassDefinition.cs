using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class DatabaseEntityMapperClassDefinition
    {
        public static CSharpClassDefinition GetDatabaseEntityMapperClassDefinition(this EntityFrameworkCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            classDefinition.Name = project.Database.GetDbEntityMapperName();

            classDefinition.BaseClass = "EntityMapper";

            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.UseMefForEntitiesMapping)
            {
                classDefinition.Namespaces.Add("System.Composition.Hosting");
                classDefinition.Namespaces.Add("System.Reflection");

                lines.Add(new CommentLine(" Get current assembly"));
                lines.Add(new CodeLine("var currentAssembly = typeof({0}).GetTypeInfo().Assembly;", project.Database.GetDbContextName()));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Get configuration for container from current assembly"));
                lines.Add(new CodeLine("var configuration = new ContainerConfiguration().WithAssembly(currentAssembly);"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Create container for exports"));
                lines.Add(new CodeLine("using (var container = configuration.CreateContainer())"));
                lines.Add(new CodeLine("{"));
                lines.Add(new CommentLine(1, " Get all definitions that implement IEntityMap interface"));
                lines.Add(new CodeLine(1, "Configurations = container.GetExports<IEntityTypeConfiguration>();"));
                lines.Add(new CodeLine("}"));
            }
            else
            {
                classDefinition.Namespaces.Add("System.Collections.Generic");

                lines.Add(new CodeLine("Configurations = new List<IEntityTypeConfiguration>()"));

                lines.Add(new CodeLine("{"));

                for (var i = 0; i < project.Database.Tables.Count; i++)
                {
                    var table = project.Database.Tables[i];

                    lines.Add(new CodeLine(1, "new {0}(){1}", table.GetEntityTypeConfigurationName(), i == project.Database.Tables.Count - 1 ? string.Empty : ","));
                }

                lines.Add(new CodeLine("};"));
            }

            classDefinition.Constructors.Add(new ClassConstructorDefinition { Lines = lines });

            return classDefinition;
        }
    }
}
