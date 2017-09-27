using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class DatabaseMapperClassDefinition : CSharpClassDefinition
    {
        public DatabaseMapperClassDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public void Init()
        {
            Namespace = Project.GetDataLayerMappingNamespace();

            Name = Project.Database.GetDbEntityMapperName();

            BaseClass = "EntityMapper";

            var lines = new List<ILine>();

            if (Project.Settings.UseMefForEntitiesMapping)
            {
                Namespaces.Add("System.Composition.Hosting");
                Namespaces.Add("System.Reflection");

                lines.Add(new CommentLine(" Get current assembly"));
                lines.Add(new CodeLine("var currentAssembly = typeof({0}).GetTypeInfo().Assembly;", Project.Database.GetDbContextName()));
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
                Namespaces.Add("System.Collections.Generic");

                lines.Add(new CodeLine("Mappings = new List<IEntityMap>()"));

                lines.Add(new CodeLine("{{"));

                for (var i = 0; i < Project.Database.Tables.Count; i++)
                {
                    var item = Project.Database.Tables[i];

                    lines.Add(new CodeLine(1, "new {0}(){1}", item.GetMapName(), i == Project.Database.Tables.Count - 1 ? String.Empty : ","));
                }

                lines.Add(new CodeLine("}};"));
            }

            Constructors.Add(new ClassConstructorDefinition
            {
                Lines = lines
            });
        }
    }
}
