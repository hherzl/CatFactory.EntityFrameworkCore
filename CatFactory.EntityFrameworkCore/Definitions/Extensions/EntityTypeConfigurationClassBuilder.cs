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

            classDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            classDefinition.Name = table.GetEntityTypeConfigurationName();

            classDefinition.Implements.Add(string.Format("IEntityTypeConfiguration<{0}>", table.GetEntityName()));

            var mapLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(table.Schema))
                mapLines.Add(new CodeLine("builder.ToTable(\"{0}\");", table.Name));
            else
                mapLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", table.Name, table.Schema));

            mapLines.Add(new CodeLine());

            var columns = default(List<Column>);

            if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
            {
                mapLines.Add(LineHelper.Warning("Add configuration for entity's key"));
                mapLines.Add(new CodeLine());
            }
            else
            {
                mapLines.Add(new CommentLine(" Set key for entity"));

                if (table.PrimaryKey.Key.Count == 1)
                {
                    mapLines.Add(new CodeLine("builder.HasKey(p => p.{0});", classDefinition.NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    mapLines.Add(new CodeLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    mapLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table.Identity != null)
            {
                mapLines.Add(new CommentLine(" Set identity for entity (auto increment)"));
                mapLines.Add(new CodeLine("builder.Property(p => p.{0}).UseSqlServerIdentityColumn();", classDefinition.NamingConvention.GetPropertyName(table.Identity.Name)));
                mapLines.Add(new CodeLine());
            }

            columns = table.Columns;

            mapLines.Add(new CommentLine(" Set configuration for columns"));

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
                    lines.Add(column.Length == 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                else if (project.Database.ColumnIsDecimal(column))
                    lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                    lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                else
                    lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                if (!column.Nullable)
                    lines.Add("IsRequired()");

                mapLines.Add(new CodeLine("{0};", string.Join(".", lines)));
            }

            mapLines.Add(new CodeLine());

            var projectSelection = project.GetSelection(table);

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken) && string.Compare(column.Name, projectSelection.Settings.ConcurrencyToken) == 0)
                {
                    mapLines.Add(new CommentLine(" Set concurrency token for entity"));
                    mapLines.Add(new CodeLine("builder"));
                    mapLines.Add(new CodeLine(1, ".Property(p => p.{0})", column.GetPropertyName()));
                    mapLines.Add(new CodeLine(1, ".ValueGeneratedOnAddOrUpdate()"));
                    mapLines.Add(new CodeLine(1, ".IsConcurrencyToken();"));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table.Uniques.Count > 0)
            {
                mapLines.Add(new CommentLine(" Add configuration for uniques"));

                foreach (var unique in table.Uniques)
                {
                    mapLines.Add(new CodeLine("builder"));

                    if (unique.Key.Count == 1)
                    {
                        mapLines.Add(new CodeLine(1, ".HasIndex(p => p.{0})", classDefinition.NamingConvention.GetPropertyName(unique.Key.First())));
                        mapLines.Add(new CodeLine(1, ".IsUnique()"));
                    }
                    else
                    {
                        mapLines.Add(new CodeLine(1, ".HasIndex(p => new {{ {0} }})", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
                        mapLines.Add(new CodeLine(1, ".IsUnique()"));
                    }

                    mapLines.Add(new CodeLine(1, ".HasName(\"{0}\");", unique.ConstraintName));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table.ForeignKeys.Count > 0)
            {
                mapLines.Add(new CommentLine(" Add configuration for foreign keys"));

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

                        mapLines.Add(new CodeLine("builder"));
                        mapLines.Add(new CodeLine(1, ".HasOne(p => p.{0})", foreignProperty.Name));
                        mapLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", table.GetPluralName()));
                        mapLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(foreignKey.Key.First()))));
                        mapLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        mapLines.Add(new CodeLine());
                    }
                    else
                    {
                        mapLines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }
            }

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", table.GetEntityName()), "builder"))
            {
                Lines = mapLines
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

            var mapLines = new List<ILine>
            {
                new CommentLine(" Set configuration for entity")
            };

            if (string.IsNullOrEmpty(view.Schema))
                mapLines.Add(new CodeLine("builder.ToTable(\"{0}\");", view.Name));
            else
                mapLines.Add(new CodeLine("builder.ToTable(\"{0}\", \"{1}\");", view.Name, view.Schema));

            mapLines.Add(new CodeLine());

            var primaryKeys = project.Database.Tables.Where(item => item.PrimaryKey != null).Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First()).ToList();

            var result = view.Columns.Where(item => !item.Nullable && primaryKeys.Contains(item.Name)).ToList();

            if (result.Count == 0)
                result = view.Columns.Where(item => !item.Nullable).ToList();

            mapLines.Add(new CommentLine(" Add configuration for entity's key"));
            mapLines.Add(new CodeLine("builder.HasKey(p => new {{ {0} }});", string.Join(", ", result.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item.Name))))));
            mapLines.Add(new CodeLine());

            mapLines.Add(new CommentLine(" Set configuration for columns"));

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
                    lines.Add(column.Length == 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                else if (project.Database.ColumnIsDecimal(column))
                    lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                else if (project.Database.ColumnIsDouble(column) || project.Database.ColumnIsSingle(column))
                    lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                else
                    lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));

                mapLines.Add(new CodeLine("{0};", string.Join(".", lines)));
            }

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition(string.Format("EntityTypeBuilder<{0}>", view.GetEntityName()), "builder"))
            {
                Lines = mapLines
            };

            classDefinition.Methods.Add(mapMethod);

            return classDefinition;
        }
    }
}
