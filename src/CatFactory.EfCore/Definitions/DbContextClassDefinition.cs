using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class DbContextClassDefinition : CSharpClassDefinition
    {
        public DbContextClassDefinition(ProjectFeature projectFeature)
        {
            ProjectFeature = projectFeature;

            Init();
        }

        public ProjectFeature ProjectFeature { get; }

        public override void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("Microsoft.EntityFrameworkCore");
            Namespaces.Add("Microsoft.Extensions.Options");

            Namespace = ProjectFeature.GetProject().GetDataLayerNamespace();
            Name = ProjectFeature.GetProject().Database.GetDbContextName();

            BaseClass = "Microsoft.EntityFrameworkCore.DbContext";

            if (ProjectFeature.GetProject().Settings.UseDataAnnotations)
            {
                Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                    }
                });
            }
            else
            {
                Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"), new ParameterDefinition("IEntityMapper", "entityMapper"))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                        new CodeLine("EntityMapper = entityMapper;")
                    }
                });
            }

            Properties.Add(new PropertyDefinition("String", "ConnectionString") { IsReadOnly = true });

            if (!ProjectFeature.GetProject().Settings.UseDataAnnotations)
            {
                Properties.Add(new PropertyDefinition("IEntityMapper", "EntityMapper") { IsReadOnly = true });
            }

            Methods.Add(GetOnConfiguringMethod());
            Methods.Add(GetOnModelCreatingMethod(ProjectFeature.GetProject()));

            if (ProjectFeature.GetProject().Settings.DeclareDbSetPropertiesInDbContext)
            {
                foreach (var table in ProjectFeature.GetProject().Database.Tables)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in ProjectFeature.GetProject().Database.Views)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }
        }

        public MethodDefinition GetOnConfiguringMethod()
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

        public MethodDefinition GetOnModelCreatingMethod(EfCoreProject project)
        {
            var lines = new List<ILine>();

            if (project.Settings.UseDataAnnotations)
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

            return new MethodDefinition(AccessModifier.Protected, "void", "OnModelCreating", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                IsOverride = true,
                Lines = lines
            };
        }
    }
}
