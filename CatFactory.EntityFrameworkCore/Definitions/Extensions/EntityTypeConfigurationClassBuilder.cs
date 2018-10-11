using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.Mapping;
using CatFactory.NetCore;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityTypeConfigurationClassBuilder
    {
        public static EntityTypeConfigurationClassDefinition GetEntityTypeConfigurationClassDefinition(this EntityFrameworkCoreProject project, ITable table)
        {
            var classDefinition = new EntityTypeConfigurationClassDefinition();

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore.Metadata.Builders");

            classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema));

            if (project.Database.HasDefaultSchema(table))
                classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();
            else
                classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace(table.Schema);

            classDefinition.Name = table.GetEntityTypeConfigurationName();

            classDefinition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", table.GetEntityName()));

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
                    configurationLines.Add(new CodeLine("builder.HasKey(p => p.{0});", classDefinition.NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    configurationLines.Add(new CodeLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    configurationLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
                    configurationLines.Add(new CodeLine());
                }
            }

            if (table.Identity != null)
            {
                configurationLines.Add(new CommentLine(" Set identity for entity (auto increment)"));
                configurationLines.Add(new CodeLine("builder.Property(p => p.{0}).UseSqlServerIdentityColumn();", classDefinition.NamingConvention.GetPropertyName(table.Identity.Name)));
                configurationLines.Add(new CodeLine());
            }

            columns = table.Columns;

            configurationLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var lines = new List<string>
                {
                    string.Format("builder.Property(p => p.{0})" , column.GetPropertyName())
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

                if (!column.Nullable)
                    lines.Add("IsRequired()");

                configurationLines.Add(new CodeLine("{0};", string.Join(".", lines)));
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
                        configurationLines.Add(new CodeLine(1, ".HasIndex(p => p.{0})", classDefinition.NamingConvention.GetPropertyName(unique.Key.First())));
                        configurationLines.Add(new CodeLine(1, ".IsUnique()"));
                    }
                    else
                    {
                        configurationLines.Add(new CodeLine(1, ".HasIndex(p => new {{ {0} }})", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
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
                        configurationLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", table.GetPluralName()));
                        configurationLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(foreignKey.Key.First()))));
                        configurationLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        configurationLines.Add(new CodeLine());
                    }
                    else
                    {
                        configurationLines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }
            }

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", table.GetEntityName()), "builder"))
            {
                Lines = configurationLines
            };

            classDefinition.Methods.Add(mapMethod);

            return classDefinition;
        }

        public static CSharpClassDefinition GetEntityTypeConfigurationClassDefinition(this EntityFrameworkCoreProject project, IView view)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore.Metadata.Builders");

            classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(project.Database.HasDefaultSchema(view) ? string.Empty : view.Schema));

            classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            classDefinition.Name = view.GetEntityTypeConfigurationName();

            classDefinition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", view.GetEntityName()));

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

            var result = view.Columns.Where(item => !item.Nullable && primaryKeys.Contains(item.Name)).ToList();

            if (result.Count == 0)
                result = view.Columns.Where(item => !item.Nullable).ToList();

            configurationLines.Add(new CommentLine(" Add configuration for entity's key"));
            configurationLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", result.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item.Name))))));
            configurationLines.Add(new CodeLine());

            configurationLines.Add(new CommentLine(" Set configuration for columns"));

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                var lines = new List<string>
                {
                    string.Format("builder.Property(p => p.{0})" , column.GetPropertyName())
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

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", view.GetEntityName()), "builder"))
            {
                Lines = configurationLines
            };

            classDefinition.Methods.Add(mapMethod);

            return classDefinition;
        }
    }
}
