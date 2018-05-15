using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.Mapping;
using CatFactory.NetCore;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class DbContextClassBuilder
    {
        public static DbContextClassDefinition GetDbContextClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection)
        {
            var classDefinition = new DbContextClassDefinition();

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespaces.Add(projectSelection.Settings.UseDataAnnotations ? projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace() : projectFeature.GetEntityFrameworkCoreProject().GetDataLayerConfigurationsNamespace());

            classDefinition.Namespace = projectFeature.GetEntityFrameworkCoreProject().GetDataLayerNamespace();
            classDefinition.Name = projectFeature.Project.Database.GetDbContextName();

            classDefinition.BaseClass = "DbContext";

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(string.Format("DbContextOptions<{0}>", classDefinition.Name), "options"))
            {
                Invocation = "base(options)"
            });

            classDefinition.Methods.Add(GetOnModelCreatingMethod(projectFeature.GetEntityFrameworkCoreProject()));

            if (projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in projectFeature.Project.Database.Tables)
                {
                    if (!table.HasDefaultSchema())
                        classDefinition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(table.Schema));

                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in projectFeature.Project.Database.Views)
                {
                    if (!view.HasDefaultSchema())
                        classDefinition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(view.Schema));

                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }

            return classDefinition;
        }

        private static MethodDefinition GetOnModelCreatingMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.UseDataAnnotations)
            {
                var primaryKeys = project.Database.Tables.Where(item => item.PrimaryKey != null).Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First()).ToList();

                foreach (var view in project.Database.Views)
                {
                    var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

                    if (result.Count == 0)
                        lines.Add(LineHelper.Warning(" Add configuration for {0} entity", view.GetEntityName()));
                    else
                        lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", view.GetEntityName(), string.Join(", ", result.Select(item => string.Format("p.{0}", NamingExtensions.namingConvention.GetPropertyName(item.Name))))));
                }

                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Apply all configurations for tables"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine("modelBuilder"));

                foreach (var table in project.Database.Tables)
                {
                    lines.Add(new CodeLine(1, ".ApplyConfiguration(new {0}())", table.GetEntityTypeConfigurationName()));
                }

                lines.Add(new CodeLine(";"));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Apply all configurations for views"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine("modelBuilder"));

                foreach (var view in project.Database.Views)
                {
                    lines.Add(new CodeLine(1, ".ApplyConfiguration(new {0}())", view.GetEntityTypeConfigurationName()));
                }

                lines.Add(new CodeLine(";"));
            }

            lines.Add(new CodeLine());
            lines.Add(new CodeLine("base.OnModelCreating(modelBuilder);"));

            return new MethodDefinition(AccessModifier.Protected, "void", "OnModelCreating", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                IsOverride = true,
                Lines = lines
            };
        }
    }
}
