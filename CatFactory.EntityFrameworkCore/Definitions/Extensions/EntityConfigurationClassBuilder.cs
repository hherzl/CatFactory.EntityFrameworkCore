using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityConfigurationClassBuilder
    {
        public static EntityConfigurationClassDefinition GetEntityConfigurationClassDefinition(this EntityFrameworkCoreProject project, ITable table)
        {
            var definition = new EntityConfigurationClassDefinition
            {
                Namespaces =
                {
                    "Microsoft.EntityFrameworkCore",
                    "Microsoft.EntityFrameworkCore.Metadata.Builders"
                },
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityConfigurationName(table),
                DbObject = table
            };

            definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema));

            if (project.Database.HasDefaultSchema(table))
                definition.Namespace = project.GetDataLayerConfigurationsNamespace();
            else
                definition.Namespace = project.GetDataLayerConfigurationsNamespace(table.Schema);

            // todo: Check logic to build property's name

            var propertyType = string.Join(".", (new string[] { project.Name, project.ProjectNamespaces.EntityLayer, project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema, project.GetEntityName(table) }).Where(item => !string.IsNullOrEmpty(item)));

            definition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", propertyType));

            var configLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(table.Schema))
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\");", table.Name));
            else
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", table.Name, table.Schema));

            configLines.Add(new CodeLine());

            var columns = default(List<Column>);

            if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
            {
                configLines.Add(LineHelper.Warning("Add configuration for entity's key"));
                configLines.Add(new CodeLine());
            }
            else
            {
                configLines.Add(new CommentLine(" Set key for entity"));

                if (table.PrimaryKey.Key.Count == 1)
                {
                    configLines.Add(new CodeLine("builder.HasKey(p => p.{0});", project.CodeNamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    configLines.Add(new CodeLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    configLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(item))))));
                    configLines.Add(new CodeLine());
                }
            }

            if (table.Identity != null)
            {
                configLines.Add(new CommentLine(" Set identity for entity (auto increment)"));
                configLines.Add(new CodeLine("builder.Property(p => p.{0}).UseSqlServerIdentityColumn();", project.CodeNamingConvention.GetPropertyName(table.Identity.Name)));
                configLines.Add(new CodeLine());
            }

            columns = table.Columns;

            configLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Property(p => p.{0})", table.GetPropertyNameHack(column))
                    };

                    if (string.Compare(column.Name, column.GetPropertyName()) != 0)
                        lines.Add(string.Format("HasColumnName(\"{0}\")", column.Name));

                    if (project.Database.ColumnIsString(column))
                        lines.Add(column.Length <= 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    else if (project.Database.ColumnIsDecimal(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                    else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                    else if (project.Database.ColumnIsByteArray(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    else
                        lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                    if (!column.Nullable)
                        lines.Add("IsRequired()");

                    configLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
                else
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Ignore(p => p.{0})", table.GetPropertyNameHack(column))
                    };

                    configLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
            }

            configLines.Add(new CodeLine());

            var projectSelection = project.GetSelection(table);

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken) && string.Compare(column.Name, projectSelection.Settings.ConcurrencyToken) == 0)
                {
                    configLines.Add(new CommentLine(" Set concurrency token for entity"));
                    configLines.Add(new CodeLine("builder"));
                    configLines.Add(new CodeLine(1, ".Property(p => p.{0})", column.GetPropertyName()));
                    configLines.Add(new CodeLine(1, ".ValueGeneratedOnAddOrUpdate()"));
                    configLines.Add(new CodeLine(1, ".IsConcurrencyToken();"));
                    configLines.Add(new CodeLine());
                }
            }

            if (table.Uniques.Count > 0)
            {
                configLines.Add(new CommentLine(" Add configuration for uniques"));

                foreach (var unique in table.Uniques)
                {
                    configLines.Add(new CodeLine("builder"));

                    if (unique.Key.Count == 1)
                    {
                        configLines.Add(new CodeLine(1, ".HasIndex(p => p.{0})", project.CodeNamingConvention.GetPropertyName(unique.Key.First())));
                        configLines.Add(new CodeLine(1, ".IsUnique()"));
                    }
                    else
                    {
                        configLines.Add(new CodeLine(1, ".HasIndex(p => new {{ {0} }})", string.Join(", ", unique.Key.Select(item => string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(item))))));
                        configLines.Add(new CodeLine(1, ".IsUnique()"));
                    }

                    configLines.Add(new CodeLine(1, ".HasName(\"{0}\");", unique.ConstraintName));
                    configLines.Add(new CodeLine());
                }
            }

            if (projectSelection.Settings.DeclareNavigationProperties && table.ForeignKeys.Count > 0)
            {
                configLines.Add(new CommentLine(" Add configuration for foreign keys"));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    if (foreignKey.Key.Count == 0)
                    {
                        continue;
                    }
                    else if (foreignKey.Key.Count == 1)
                    {
                        var foreignProperty = foreignKey.GetParentNavigationProperty(foreignTable, project);

                        configLines.Add(new CodeLine("builder"));
                        configLines.Add(new CodeLine(1, ".HasOne(p => p.{0})", foreignProperty.Name));
                        configLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", project.GetNavigationPropertyName(table)));
                        configLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(foreignKey.Key.First()))));
                        configLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        configLines.Add(new CodeLine());
                    }
                    else
                    {
                        configLines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }
            }

            definition.Methods.Add(new MethodDefinition(AccessModifier.Public, "void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", propertyType), "builder"))
            {
                Lines = configLines
            });

            return definition;
        }

        public static EntityConfigurationClassDefinition GetEntityConfigurationClassDefinition(this EntityFrameworkCoreProject project, IView view)
        {
            var definition = new EntityConfigurationClassDefinition
            {
                Namespaces =
                {
                    "Microsoft.EntityFrameworkCore",
                    "Microsoft.EntityFrameworkCore.Metadata.Builders"
                },
                Namespace = project.GetDataLayerConfigurationsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityConfigurationName(view),
                Implements =
                {
                    string.Format("IEntityTypeConfiguration<{0}>", project.GetEntityName(view))
                },
                DbObject = view
            };

            definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(view) ? string.Empty : view.Schema));

            var configLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(view.Schema))
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\");", view.Name));
            else
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", view.Name, view.Schema));

            configLines.Add(new CodeLine());

            var primaryKeys = project.Database.Tables.Where(item => item.PrimaryKey != null).Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First()).ToList();

            var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            if (result.Count == 0)
                result = view.Columns.Where(item => !item.Nullable).ToList();

            configLines.Add(new CommentLine(" Add configuration for entity's key"));
            configLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", result.Select(item => string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(item.Name))))));
            configLines.Add(new CodeLine());

            configLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Property(p => p.{0})" , view.GetPropertyNameHack(column))
                    };

                    if (string.Compare(column.Name, column.GetPropertyName()) != 0)
                        lines.Add(string.Format("HasColumnName(\"{0}\")", column.Name));

                    if (project.Database.ColumnIsString(column))
                        lines.Add(column.Length <= 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    else if (project.Database.ColumnIsDecimal(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                    else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                    else
                        lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                    configLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
                else
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Ignore(p => p.{0})", view.GetPropertyNameHack(column))
                    };

                    configLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
            }

            definition.Methods.Add(new MethodDefinition(AccessModifier.Public, "void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", project.GetEntityName(view)), "builder"))
            {
                Lines = configLines
            });

            return definition;
        }
    }
}
