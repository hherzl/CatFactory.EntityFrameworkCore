using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class DbContextClassBuilder
    {
        public static DbContextClassDefinition GetDbContextClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection)
        {
            var definition = new DbContextClassDefinition
            {
                Namespace = projectFeature.GetEntityFrameworkCoreProject().GetDataLayerNamespace(),
                Name = projectFeature.Project.Database.GetDbContextName(),
                BaseClass = "DbContext"
            };

            definition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            definition.Namespaces.Add(projectSelection.Settings.UseDataAnnotations ? projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace() : projectFeature.GetEntityFrameworkCoreProject().GetDataLayerConfigurationsNamespace());

            definition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(string.Format("DbContextOptions<{0}>", definition.Name), "options"))
            {
                Invocation = "base(options)"
            });

            definition.Methods.Add(GetOnModelCreatingMethod(projectFeature.GetEntityFrameworkCoreProject()));

            if (projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in projectFeature.Project.Database.Tables)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(table))
                        definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(table.Schema));

                    definition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in projectFeature.Project.Database.Views)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(view))
                        definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(view.Schema));

                    definition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }
            else
            {
                foreach (var table in projectFeature.Project.Database.Tables)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(table))
                        definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetDataLayerConfigurationsNamespace(table.Schema));
                }

                foreach (var view in projectFeature.Project.Database.Views)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(view))
                        definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetDataLayerConfigurationsNamespace(view.Schema));
                }
            }

            foreach (var scalarFunction in projectFeature.Project.Database.ScalarFunctions)
            {
                definition.Namespaces.AddUnique("System");

                var returnType = projectFeature.Project.Database.ResolveType(scalarFunction.Parameters[0].Type);

                var method = new MethodDefinition
                {
                    Attributes =
                    {
                        new MetadataAttribute("DbFunction")
                        {
                            Sets =
                            {
                                new MetadataAttributeSet("FunctionName", string.Format("\"{0}\"", scalarFunction.Name)),
                                new MetadataAttributeSet("Schema", string.Format("\"{0}\"", scalarFunction.Schema))
                            }
                        }
                    },
                    IsStatic = true,
                    Type = returnType.HasClrAliasType ? returnType.ClrAliasType : returnType.GetClrType().Name,
                    Name = scalarFunction.GetScalarFunctionMethodName(),
                    Lines =
                    {
                        new CodeLine("throw new Exception();")
                    }
                };

                var parameters = scalarFunction.Parameters.Where(item => !string.IsNullOrEmpty(item.Name)).ToList();

                foreach (var parameter in parameters)
                {
                    method.Parameters.Add(new ParameterDefinition(parameter.Type, parameter.Name));
                }

                definition.Methods.Add(method);
            }

            return definition;
        }

        private static MethodDefinition GetOnModelCreatingMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.UseDataAnnotations)
            {
                var primaryKeys = project
                    .Database
                    .Tables
                    .Where(item => item.PrimaryKey != null)
                    .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey)
                    .Select(c => c.Name)
                    .First())
                    .ToList();

                foreach (var view in project.Database.Views)
                {
                    var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

                    if (result.Count == 0)
                    {
                        lines.Add(
                            new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});",
                                view.GetEntityName(),
                                string.Join(", ", view.Columns.Select(item => string.Format("p.{0}", NamingExtensions.namingConvention.GetPropertyName(item.Name))))
                            )
                        );

                        lines.Add(new CodeLine());
                    }
                    else
                    {
                        lines.Add(
                            new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});",
                                view.GetEntityName(),
                                string.Join(", ", result.Select(item => string.Format("p.{0}", NamingExtensions.namingConvention.GetPropertyName(item.Name))))
                            )
                        );

                        lines.Add(new CodeLine());
                    }
                }
            }
            else
            {
                if (project.Database.Tables.Count > 0)
                {
                    lines.Add(new CommentLine(" Apply all configurations for tables"));
                    lines.Add(new CodeLine());

                    lines.Add(new CodeLine("modelBuilder"));

                    foreach (var table in project.Database.Tables)
                    {
                        lines.Add(new CodeLine(1, ".ApplyConfiguration(new {0}())", table.GetEntityConfigurationName()));
                    }

                    lines.Add(new CodeLine(";"));

                    lines.Add(new CodeLine());
                }

                if (project.Database.Views.Count > 0)
                {
                    lines.Add(new CommentLine(" Apply all configurations for views"));
                    lines.Add(new CodeLine());

                    lines.Add(new CodeLine("modelBuilder"));

                    foreach (var view in project.Database.Views)
                    {
                        lines.Add(new CodeLine(1, ".ApplyConfiguration(new {0}())", view.GetEntityConfigurationName()));
                    }

                    lines.Add(new CodeLine(";"));
                    lines.Add(new CodeLine());
                }
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
