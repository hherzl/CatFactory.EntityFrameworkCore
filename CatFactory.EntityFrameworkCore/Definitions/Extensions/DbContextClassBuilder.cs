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
            var classDefinition = new DbContextClassDefinition
            {
                Namespace = projectFeature.GetEntityFrameworkCoreProject().GetDataLayerNamespace(),
                Name = projectFeature.Project.Database.GetDbContextName(),
                BaseClass = "DbContext"
            };

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespaces.Add(projectSelection.Settings.UseDataAnnotations ? projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace() : projectFeature.GetEntityFrameworkCoreProject().GetDataLayerConfigurationsNamespace());

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(string.Format("DbContextOptions<{0}>", classDefinition.Name), "options"))
            {
                Invocation = "base(options)"
            });

            classDefinition.Methods.Add(GetOnModelCreatingMethod(projectFeature.GetEntityFrameworkCoreProject()));

            if (projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in projectFeature.Project.Database.Tables)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(table))
                        classDefinition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(table.Schema));

                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", table.GetEntityName()), table.GetPluralName()));
                }

                foreach (var view in projectFeature.Project.Database.Views)
                {
                    if (!projectFeature.Project.Database.HasDefaultSchema(view))
                        classDefinition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(view.Schema));

                    classDefinition.Properties.Add(new PropertyDefinition(string.Format("DbSet<{0}>", view.GetEntityName()), view.GetPluralName()));
                }
            }

            foreach (var scalarFunction in projectFeature.Project.Database.ScalarFunctions)
            {
                classDefinition.Namespaces.AddUnique("System");

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
                    Lines = new List<ILine>
                    {
                        new CodeLine("throw new Exception();")
                    }
                };

                var parameters = scalarFunction.Parameters.Where(item => !string.IsNullOrEmpty(item.Name)).ToList();

                foreach (var parameter in parameters)
                {
                    method.Parameters.Add(new ParameterDefinition(parameter.Type, parameter.Name));
                }

                classDefinition.Methods.Add(method);
            }

            return classDefinition;
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
                        //lines.Add(LineHelper.Warning(" Add configuration for {0} entity", view.GetEntityName()));
                        lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", view.GetEntityName(), string.Join(", ", view.Columns.Select(item => string.Format("p.{0}", NamingExtensions.namingConvention.GetPropertyName(item.Name))))));
                        lines.Add(new CodeLine());
                    }
                    else
                    {
                        lines.Add(new CodeLine("modelBuilder.Entity<{0}>().HasKey(p => new {{ {1} }});", view.GetEntityName(), string.Join(", ", result.Select(item => string.Format("p.{0}", NamingExtensions.namingConvention.GetPropertyName(item.Name))))));
                        lines.Add(new CodeLine());
                    }
                }
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
