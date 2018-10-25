using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
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
                Name = table.GetEntityConfigurationName()
            };
            
            definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema));

            if (project.Database.HasDefaultSchema(table))
                definition.Namespace = project.GetDataLayerConfigurationsNamespace();
            else
                definition.Namespace = project.GetDataLayerConfigurationsNamespace(table.Schema);

            // todo: Check logic to build property's name

            var propertyType = string.Join(".", (new string[] { project.Name, project.Namespaces.EntityLayer, project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema, table.GetEntityName() }).Where(item => !string.IsNullOrEmpty(item)));

            definition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", propertyType));

            var configurationLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(table.Schema))
                configurationLines.Add(new CodeLine("builder.ToTable(\"{0}\");", table.Name));
            else
                configurationLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", table.Name, table.Schema));

            configurationLines.Add(new CodeLine());

            var columns = default(List<Column>);

            if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
            {
                configurationLines.Add(LineHelper.Warning("Add configuration for entity's key"));
                configurationLines.Add(new CodeLine());
            }
            else
            {
                configurationLines.Add(new CommentLine(" Set key for entity"));

                if (table.PrimaryKey.Key.Count == 1)
                {
                    configurationLines.Add(new CodeLine("builder.HasKey(p => p.{0});", definition.NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    configurationLines.Add(new CodeLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    configurationLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", definition.NamingConvention.GetPropertyName(item))))));
                    configurationLines.Add(new CodeLine());
                }
            }

            if (table.Identity != null)
            {
                configurationLines.Add(new CommentLine(" Set identity for entity (auto increment)"));
                configurationLines.Add(new CodeLine("builder.Property(p => p.{0}).UseSqlServerIdentityColumn();", definition.NamingConvention.GetPropertyName(table.Identity.Name)));
                configurationLines.Add(new CodeLine());
            }

            columns = table.Columns;

            configurationLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (project.Database.ColumnHasTypeMappedToClr(column))
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

                    configurationLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
                else
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Ignore(p => p.{0})", table.GetPropertyNameHack(column))
                    };

                    configurationLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }

                var type = project.Database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == column.Type);
            }

            configurationLines.Add(new CodeLine());

            var projectSelection = project.GetSelection(table);

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken) && string.Compare(column.Name, projectSelection.Settings.ConcurrencyToken) == 0)
                {
                    configurationLines.Add(new CommentLine(" Set concurrency token for entity"));
                    configurationLines.Add(new CodeLine("builder"));
                    configurationLines.Add(new CodeLine(1, ".Property(p => p.{0})", column.GetPropertyName()));
                    configurationLines.Add(new CodeLine(1, ".ValueGeneratedOnAddOrUpdate()"));
                    configurationLines.Add(new CodeLine(1, ".IsConcurrencyToken();"));
                    configurationLines.Add(new CodeLine());
                }
            }

            if (table.Uniques.Count > 0)
            {
                configurationLines.Add(new CommentLine(" Add configuration for uniques"));

                foreach (var unique in table.Uniques)
                {
                    configurationLines.Add(new CodeLine("builder"));

                    if (unique.Key.Count == 1)
                    {
                        configurationLines.Add(new CodeLine(1, ".HasIndex(p => p.{0})", definition.NamingConvention.GetPropertyName(unique.Key.First())));
                        configurationLines.Add(new CodeLine(1, ".IsUnique()"));
                    }
                    else
                    {
                        configurationLines.Add(new CodeLine(1, ".HasIndex(p => new {{ {0} }})", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", definition.NamingConvention.GetPropertyName(item))))));
                        configurationLines.Add(new CodeLine(1, ".IsUnique()"));
                    }

                    configurationLines.Add(new CodeLine(1, ".HasName(\"{0}\");", unique.ConstraintName));
                    configurationLines.Add(new CodeLine());
                }
            }

            if (table.ForeignKeys.Count > 0)
            {
                configurationLines.Add(new CommentLine(" Add configuration for foreign keys"));

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

                        configurationLines.Add(new CodeLine("builder"));
                        configurationLines.Add(new CodeLine(1, ".HasOne(p => p.{0})", foreignProperty.Name));
                        configurationLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", table.GetNavigationPropertyName()));
                        configurationLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", string.Format("p.{0}", definition.NamingConvention.GetPropertyName(foreignKey.Key.First()))));
                        configurationLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        configurationLines.Add(new CodeLine());
                    }
                    else
                    {
                        configurationLines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }
            }

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", propertyType), "builder"))
            {
                Lines = configurationLines
            };

            definition.Methods.Add(mapMethod);

            return definition;
        }

        public static CSharpClassDefinition GetEntityTypeConfigurationClassDefinition(this EntityFrameworkCoreProject project, IView view)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            definition.Namespaces.Add("Microsoft.EntityFrameworkCore.Metadata.Builders");

            definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(view) ? string.Empty : view.Schema));

            definition.Namespace = project.GetDataLayerConfigurationsNamespace();

            definition.Name = view.GetEntityConfigurationName();

            definition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", view.GetEntityName()));

            var configurationLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(view.Schema))
                configurationLines.Add(new CodeLine("builder.ToTable(\"{0}\");", view.Name));
            else
                configurationLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", view.Name, view.Schema));

            configurationLines.Add(new CodeLine());

            var primaryKeys = project.Database.Tables.Where(item => item.PrimaryKey != null).Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First()).ToList();

            var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            if (result.Count == 0)
                result = view.Columns.Where(item => !item.Nullable).ToList();

            configurationLines.Add(new CommentLine(" Add configuration for entity's key"));
            configurationLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", result.Select(item => string.Format("p.{0}", definition.NamingConvention.GetPropertyName(item.Name))))));
            configurationLines.Add(new CodeLine());

            configurationLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                if (project.Database.ColumnHasTypeMappedToClr(column))
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

                    configurationLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
                else
                {
                    var lines = new List<string>
                    {
                        string.Format("builder.Ignore(p => p.{0})", view.GetPropertyNameHack(column))
                    };

                    configurationLines.Add(new CodeLine("{0};", string.Join(".", lines)));
                }
            }

            var configureMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", view.GetEntityName()), "builder"))
            {
                Lines = configurationLines
            };

            definition.Methods.Add(configureMethod);

            return definition;
        }
    }
}
