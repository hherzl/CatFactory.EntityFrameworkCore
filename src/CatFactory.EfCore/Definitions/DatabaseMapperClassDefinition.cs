using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class DatabaseMapperClassDefinition
    {
        public static CSharpClassDefinition GetDatabaseMapperClassDefinition(this EfCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespace = project.GetDataLayerMappingNamespace();

            classDefinition.Name = project.Database.GetDbEntityMapperName();

            classDefinition.BaseClass = "EntityMapper";

            var lines = new List<ILine>();

            if (project.Settings.UseMefForEntitiesMapping)
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
                lines.Add(new CodeLine("{{"));
                lines.Add(new CommentLine(1, " Get all definitions that implement IEntityMap interface"));
                lines.Add(new CodeLine(1, "Mappings = container.GetExports<IEntityMap>();"));
                lines.Add(new CodeLine("}}"));
            }
            else
            {
                classDefinition.Namespaces.Add("System.Collections.Generic");

                lines.Add(new CodeLine("Mappings = new List<IEntityMap>()"));

                lines.Add(new CodeLine("{{"));

                for (var i = 0; i < project.Database.Tables.Count; i++)
                {
                    var item = project.Database.Tables[i];

                    lines.Add(new CodeLine(1, "new {0}(){1}", item.GetMapName(), i == project.Database.Tables.Count - 1 ? String.Empty : ","));
                }

                lines.Add(new CodeLine("}};"));
            }

            classDefinition.Constructors.Add(new ClassConstructorDefinition { Lines = lines });

            return classDefinition;
        }
    }
}
