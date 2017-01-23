using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class DbContextClassDefinition : CSharpClassDefinition
    {
        public DbContextClassDefinition(EfCoreProject project, ProjectFeature projectFeature)
        {
            Namespaces.Add("System");
            Namespaces.Add("Microsoft.EntityFrameworkCore");
            Namespaces.Add("Microsoft.Extensions.Options");

            Name = project.Database.GetDbContextName();

            BaseClass = "Microsoft.EntityFrameworkCore.DbContext";

            if (project.UseDataAnnotations)
            {
                Constructors.Add(new ClassConstructorDefinition()
                {
                    Parameters = new List<ParameterDefinition>()
                    {
                        new ParameterDefinition("IOptions<AppSettings>", "appSettings")
                    },
                    Lines = new List<CodeLine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                    }
                });
            }
            else
            {
                Constructors.Add(new ClassConstructorDefinition()
                {
                    Parameters = new List<ParameterDefinition>()
                    {
                        new ParameterDefinition("IOptions<AppSettings>", "appSettings"),
                        new ParameterDefinition("IEntityMapper", "entityMapper")
                    },
                    Lines = new List<CodeLine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                        new CodeLine("EntityMapper = entityMapper;")
                    }
                });
            }

            Properties.Add(new PropertyDefinition("String", "ConnectionString"));

            if (!project.UseDataAnnotations)
            {
                Properties.Add(new PropertyDefinition("IEntityMapper", "EntityMapper"));
            }

            Methods.Add(GetOnConfiguringMethod());
            Methods.Add(GetOnModelCreatingMethod(project));

            if (project.DeclareDbSetPropertiesInDbContext)
            {
                foreach (var table in project.Database.Tables)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", table.GetEntityName()), table.GetEntityName()));
                }

                foreach (var view in project.Database.Views)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", view.GetEntityName()), view.GetEntityName()));
                }
            }
        }

        public MethodDefinition GetOnConfiguringMethod()
        {
            return new MethodDefinition("void", "OnConfiguring")
            {
                AccessModifier = AccessModifier.Protected,
                Prefix = "override",
                Parameters = new List<ParameterDefinition>() { new ParameterDefinition("DbContextOptionsBuilder", "optionsBuilder") },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("optionsBuilder.UseSqlServer(ConnectionString);"),
                    new CodeLine(),
                    new CodeLine("base.OnConfiguring(optionsBuilder);")
                }
            };
        }

        public MethodDefinition GetOnModelCreatingMethod(EfCoreProject project)
        {
            var lines = new List<CodeLine>();

            if (project.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    if (table.PrimaryKey?.Key.Count > 1)
                    {
                        lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", table.GetEntityName(), String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", item.Name)))));
                        lines.Add(new CodeLine());
                    }
                }

                foreach (var view in project.Database.Views)
                {
                    lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", view.GetEntityName(), String.Join(", ", view.Columns.Select(item => String.Format("p.{0}", item.Name)))));
                    lines.Add(new CodeLine());
                }
            }
            else
            {
                lines.Add(new CodeLine("EntityMapper.MapEntities(modelBuilder);"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("base.OnModelCreating(modelBuilder);"));

            return new MethodDefinition("void", "OnModelCreating")
            {
                AccessModifier = AccessModifier.Protected,
                Prefix = "override",
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("ModelBuilder", "modelBuilder")
                },
                Lines = lines
            };
        }
    }
}
