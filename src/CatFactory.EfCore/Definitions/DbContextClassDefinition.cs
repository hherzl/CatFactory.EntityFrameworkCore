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

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            classDefinition.Namespaces.Add("Microsoft.Extensions.Options");

            if (projectFeature.GetEfCoreProject().Settings.UseDataAnnotations)
            {
                classDefinition.Namespaces.Add(projectFeature.GetEfCoreProject().GetEntityLayerNamespace());
            }
            else
            {
                classDefinition.Namespaces.Add(projectFeature.GetEfCoreProject().GetDataLayerMappingNamespace());
            }

            classDefinition.Namespace = projectFeature.GetEfCoreProject().GetDataLayerNamespace();
            classDefinition.Name = projectFeature.GetEfCoreProject().Database.GetDbContextName();

            classDefinition.BaseClass = "Microsoft.EntityFrameworkCore.DbContext";

            if (projectFeature.GetEfCoreProject().Settings.UseDataAnnotations)
            {
                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                    }
                });
            }
            else
            {
                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"), new ParameterDefinition("IEntityMapper", "entityMapper"))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                        new CodeLine("EntityMapper = entityMapper;")
                    }
                });
            }

            classDefinition.Properties.Add(new PropertyDefinition("String", "ConnectionString") { IsReadOnly = true });

            if (!projectFeature.GetEfCoreProject().Settings.UseDataAnnotations)
            {
                classDefinition.Properties.Add(new PropertyDefinition("IEntityMapper", "EntityMapper") { IsReadOnly = true });
            }

            classDefinition.Methods.Add(GetOnConfiguringMethod());
            classDefinition.Methods.Add(GetOnModelCreatingMethod(projectFeature.GetEfCoreProject()));

            if (projectFeature.GetEfCoreProject().Settings.DeclareDbSetPropertiesInDbContext)
            {
                foreach (var table in projectFeature.GetEfCoreProject().Database.Tables)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in projectFeature.GetEfCoreProject().Database.Views)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }

            return classDefinition;
        }

        private static MethodDefinition GetOnConfiguringMethod()
        {
            return new MethodDefinition(AccessModifier.Protected, "void", "OnConfiguring", new ParameterDefinition("DbContextOptionsBuilder", "optionsBuilder"))
            {
                IsOverride = true,
                Lines = new List<ILine>()
                {
                    new CodeLine("optionsBuilder.UseSqlServer(ConnectionString);"),
                    new CodeLine(),
                    new CodeLine("base.OnConfiguring(optionsBuilder);")
                }
            };
        }

        private static MethodDefinition GetOnModelCreatingMethod(EfCoreProject project)
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
                lines.Add(new CodeLine("EntityMapper.MapEntities(modelBuilder);"));
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
