using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class DbContextClassDefinition
    {
        public static CSharpClassDefinition GetDbContextClassDefinition(this ProjectFeature projectFeature)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().Settings.UseDataAnnotations ? projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace() : projectFeature.GetEntityFrameworkCoreProject().GetDataLayerMappingNamespace());

            classDefinition.Namespace = projectFeature.GetEntityFrameworkCoreProject().GetDataLayerNamespace();
            classDefinition.Name = projectFeature.Project.Database.GetDbContextName();

            classDefinition.BaseClass = "DbContext";

            if (projectFeature.GetEntityFrameworkCoreProject().Settings.UseDataAnnotations)
            {
                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(string.Format("DbContextOptions<{0}>", classDefinition.Name), "options"))
                {
                    Invocation = "base(options)"
                });
            }
            else
            {
                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(string.Format("DbContextOptions<{0}>", classDefinition.Name), "options"), new ParameterDefinition("IEntityMapper", "entityMapper"))
                {
                    Invocation = "base(options)",
                    Lines = new List<ILine>()
                    {
                        new CodeLine("EntityMapper = entityMapper;")
                    }
                });
            }

            if (!projectFeature.GetEntityFrameworkCoreProject().Settings.UseDataAnnotations)
            {
                classDefinition.Properties.Add(new PropertyDefinition("IEntityMapper", "EntityMapper") { IsReadOnly = true });
            }

            classDefinition.Methods.Add(GetOnModelCreatingMethod(projectFeature.GetEntityFrameworkCoreProject()));

            if (projectFeature.GetEntityFrameworkCoreProject().Settings.DeclareDbSetPropertiesInDbContext)
            {
                foreach (var table in projectFeature.Project.Database.Tables)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in projectFeature.Project.Database.Views)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }

            return classDefinition;
        }

        private static MethodDefinition GetOnModelCreatingMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            if (project.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    if (table.PrimaryKey?.Key.Count > 1)
                    {
                        lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", table.GetEntityName(), string.Join(", ", table.Columns.Select(item => string.Format("p.{0}", item.Name)))));
                        lines.Add(new CodeLine());
                    }
                }

                foreach (var view in project.Database.Views)
                {
                    lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", view.GetEntityName(), string.Join(", ", view.Columns.Select(item => string.Format("p.{0}", item.Name)))));
                    lines.Add(new CodeLine());
                }
            }
            else
            {
                lines.Add(new CommentLine(" This feature will be change for EF Core 2"));
                lines.Add(new CodeLine("EntityMapper.ConfigureEntities(modelBuilder);"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("base.OnModelCreating(modelBuilder);"));

            return new MethodDefinition(AccessModifier.Protected, "void", "OnModelCreating", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                IsOverride = true,
                Lines = lines
            };
        }
    }
}
