using System;
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
                Namespace = project.Database.HasDefaultSchema(table) ? project.GetDataLayerConfigurationsNamespace() : project.GetDataLayerConfigurationsNamespace(table.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityConfigurationName(table),
                DbObject = table
            };

            var projectSelection = project.GetSelection(table);

            definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema));

            // todo: Check logic to build property's name

            var propertyType = "";

            if (table.HasSameEnclosingName())
                propertyType = string.Join(".", (new string[] { project.ProjectNamespaces.EntityLayer, project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema, project.GetEntityName(table) }).Where(item => !string.IsNullOrEmpty(item)));
            else
                propertyType = project.GetEntityName(table);

            definition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", propertyType));

            var configLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(table.Schema))
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\");", table.Name));
            else
                configLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", table.Name, table.Schema));

            configLines.Add(new EmptyLine());

            var columns = default(List<Column>);

            if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
            {
                configLines.Add(LineHelper.Warning("Add configuration for entity's key"));
                configLines.Add(new EmptyLine());
            }
            else
            {
                configLines.Add(new CommentLine(" Set key for entity"));

                if (table.PrimaryKey.Key.Count == 1)
                {
                    configLines.Add(new CodeLine("builder.HasKey(p => p.{0});", project.CodeNamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    configLines.Add(new EmptyLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    configLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(item))))));
                    configLines.Add(new EmptyLine());
                }
            }

            if (table.Identity != null)
            {
                var identityColumnMethod = projectSelection.Settings.EfCoreTargetVersion == EfCoreVersion.EF2 ? "UseSqlServerIdentityColumn" : "UseIdentityColumn";
                configLines.Add(new CommentLine(" Set identity for entity (auto increment)"));
                configLines.Add(new CodeLine("builder.Property(p => p.{0}).{1}();", project.CodeNamingConvention.GetPropertyName(table.Identity.Name), identityColumnMethod));
                configLines.Add(new EmptyLine());
            }

            columns = table.Columns;

            configLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var valueConversion = default(Type);

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var lines = new List<string>
                    {
                        string.Format("Property(p => p.{0})", project.GetPropertyName(table, column))
                    };

                    if (string.Compare(column.Name, project.GetPropertyName(table, column)) != 0)
                        lines.Add(string.Format("HasColumnName(\"{0}\")", column.Name));

                    if (project.Database.ColumnIsByteArray(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    else if (project.Database.ColumnIsDecimal(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                    else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                    else if (project.Database.ColumnIsString(column))
                    {
                        if (column.Length <= 0)
                        {
                            lines.Add(string.Format("HasColumnType(\"{0}(max)\")", column.Type));
                        }
                        else
                        {
                            lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));
                            lines.Add(string.Format("HasMaxLength({0})", column.Length));
                        }
                    }
                    else
                        lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                    // Use ValueConversionMaps to detect and apply ValueConversion Type based on Type

                    if (project.ValueConversionMaps.TryGetValue(column.Type, out valueConversion) == true)
                        lines.Add($".HasConversion(typeof({valueConversion?.FullName}))");

                    if (!column.Nullable)
                        lines.Add("IsRequired()");

                    configLines.Add(new CodeLine("builder"));

                    foreach (var line in lines)
                    {
                        configLines.Add(new CodeLine(1, ".{0}", line));
                    }

                    configLines.Add(new CodeLine(1, ";"));
                    configLines.Add(new EmptyLine());
                }
                else
                {
                    configLines.Add(new CodeLine("builder.Ignore(p => p.{0});", project.GetPropertyName(table, column)));
                    configLines.Add(new EmptyLine());
                }
            }

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken) && string.Compare(column.Name, projectSelection.Settings.ConcurrencyToken) == 0)
                {
                    configLines.Add(new CommentLine(" Set concurrency token for entity"));
                    configLines.Add(new CodeLine("builder"));
                    configLines.Add(new CodeLine(1, ".Property(p => p.{0})", project.GetPropertyName(table, column)));
                    configLines.Add(new CodeLine(1, ".ValueGeneratedOnAddOrUpdate()"));
                    configLines.Add(new CodeLine(1, ".IsConcurrencyToken();"));
                    configLines.Add(new EmptyLine());
                }
            }

            if (projectSelection.Settings.AddConfigurationForUniquesInFluentAPI && table.Uniques.Count > 0)
            {
                configLines.Add(new CommentLine(" Add configuration for uniques"));
                configLines.Add(new EmptyLine());

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
                    configLines.Add(new EmptyLine());
                }
            }

            if (projectSelection.Settings.AddConfigurationForForeignKeysInFluentAPI && projectSelection.Settings.DeclareNavigationProperties && table.ForeignKeys.Count > 0)
            {
                configLines.Add(new CommentLine(" Add configuration for foreign keys"));
                configLines.Add(new EmptyLine());

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null || foreignKey.Key.Count == 0)
                        continue;

                    if (foreignKey.Key.Count == 1)
                    {
                        var foreignProperty = foreignKey.GetParentNavigationProperty(foreignTable, project);

                        configLines.Add(new CodeLine("builder"));
                        configLines.Add(new CodeLine(1, ".HasOne(p => p.{0})", foreignProperty.Name));
                        configLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", project.GetNavigationPropertyName(table)));
                        configLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(foreignKey.Key.First()))));
                        configLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        configLines.Add(new EmptyLine());
                    }
                    else
                    {
                        configLines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }
            }

            if (projectSelection.Settings.AddConfigurationForDefaultsInFluentAPI && table.Defaults.Count > 0)
            {
                configLines.Add(new CommentLine(" Add configuration for defaults"));
                configLines.Add(new EmptyLine());

                foreach (var def in table.Defaults)
                {
                    var propertyName = def.Key.First();

                    configLines.Add(new CodeLine("builder"));
                    configLines.Add(new CodeLine(1, ".Property(p => p.{0})", project.GetPropertyName(propertyName)));
                    configLines.Add(new CodeLine(1, ".HasDefaultValueSql(\"{0}\");", def.Value));
                    configLines.Add(new EmptyLine());
                }
            }

            definition.Methods.Add(new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = "void",
                Name = "Configure",
                Parameters =
                {
                    new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", propertyType), "builder")
                },
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
                Namespace = project.Database.HasDefaultSchema(view) ? project.GetDataLayerConfigurationsNamespace() : project.GetDataLayerConfigurationsNamespace(view.Schema),
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

            configLines.Add(new EmptyLine());

            var primaryKeys = project
                .Database
                .Tables
                .Where(item => item.PrimaryKey != null)
                .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First())
                .ToList();

            var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            if (result.Count == 0)
                result = view.Columns.Where(item => !item.Nullable).ToList();

            configLines.Add(new CommentLine(" Add configuration for entity's key"));

            if (result.Count == 1)
                configLines.Add(new CodeLine("builder.HasKey(p => {0});", string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(result.First().Name))));
            else
                configLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", result.Select(item => string.Format("p.{0}", project.CodeNamingConvention.GetPropertyName(item.Name))))));

            configLines.Add(new EmptyLine());

            configLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                var valueConversion = default(Type);

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var lines = new List<string>
                    {
                        string.Format("Property(p => p.{0})" , project.GetPropertyName( view, column))
                    };

                    if (string.Compare(column.Name, project.GetPropertyName(view, column)) != 0)
                        lines.Add(string.Format("HasColumnName(\"{0}\")", column.Name));

                    else if (project.Database.ColumnIsDecimal(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                    else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                        lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                    if (project.Database.ColumnIsString(column))
                        lines.Add(column.Length <= 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    else
                        lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                    // Use ValueConversionMaps to detect and apply ValueConversion Type based on Type

                    if (project.ValueConversionMaps?.TryGetValue(column.Type, out valueConversion) == true)
                        lines.Add($"HasConversion(typeof({valueConversion?.FullName}))");

                    configLines.Add(new CodeLine("builder"));

                    foreach (var line in lines)
                    {
                        configLines.Add(new CodeLine(1, ".{0}", line));
                    }

                    configLines.Add(new CodeLine(1, ";"));
                    configLines.Add(new EmptyLine());
                }
                else
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Ignore(p => p.{0})", project.GetPropertyName( view, column))
                    };

                    configLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
            }

            definition.Methods.Add(new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = "void",
                Name = "Configure",
                Parameters =
                {
                    new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", project.GetEntityName(view)), "builder")
                },
                Lines = configLines
            });

            return definition;
        }
    }
}
