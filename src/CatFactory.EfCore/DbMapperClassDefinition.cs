using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class DbMapperClassDefinition : CSharpClassDefinition
    {
        public DbMapperClassDefinition(EfCoreProject project)
        {
            Name = project.Database.GetDbEntityMapperName();

            BaseClass = "EntityMapper";

            var lines = new List<CodeLine>();

            if (project.Settings.UseMefForEntitiesMapping)
            {
                Namespaces.Add("System.Composition.Hosting");
                Namespaces.Add("System.Reflection");

                lines.Add(new CodeLine("var configuration = new ContainerConfiguration().WithAssembly(typeof(StoreDbContext).GetTypeInfo().Assembly);"));
                lines.Add(new CodeLine());
                lines.Add(new CodeLine("using (var container = configuration.CreateContainer())"));
                lines.Add(new CodeLine("{{"));
                lines.Add(new CodeLine(1, "Mappings = container.GetExports<IEntityMap>();"));
                lines.Add(new CodeLine("}}"));
            }
            else
            {
                Namespaces.Add("System.Collections.Generic");

                lines.Add(new CodeLine("Mappings = new List<IEntityMap>()"));

                lines.Add(new CodeLine("{{"));

                for (var i = 0; i < project.Database.Tables.Count; i++)
                {
                    var item = project.Database.Tables[i];

                    lines.Add(new CodeLine(1, "new {0}(){1}", item.GetMapName(), i == project.Database.Tables.Count - 1 ? String.Empty : ","));
                }

                lines.Add(new CodeLine("}};"));
            }

            Constructors.Add(new ClassConstructorDefinition()
            {
                Lines = lines
            });
        }
    }
}
