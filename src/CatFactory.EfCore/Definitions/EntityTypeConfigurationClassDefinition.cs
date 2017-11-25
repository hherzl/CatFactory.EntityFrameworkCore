using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityTypeConfigurationClassDefinition
    {
        public static CSharpClassDefinition GetEntityTypeConfigurationClassDefinition(this ITable table, EntityFrameworkCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            if (project.Settings.UseMefForEntitiesMapping)
            {
                classDefinition.Namespaces.Add("System.Composition");

                classDefinition.Attributes.Add(new MetadataAttribute("Export", "typeof(IEntityTypeConfiguration)"));
            }

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(table.HasDefaultSchema() ? string.Empty : table.Schema));

            classDefinition.Namespace = project.GetDataLayerMappingNamespace();

            classDefinition.Name = table.GetEntityTypeConfigurationName();

            classDefinition.Implements.Add("IEntityTypeConfiguration");

            var mapLines = new List<ILine>();

            mapLines.Add(new CodeLine("modelBuilder.Entity<{0}>(builder =>", table.GetSingularName()));
            mapLines.Add(new CodeLine("{"));

            mapLines.Add(new CommentLine(1, " Set configuration for entity"));

            if (string.IsNullOrEmpty(table.Schema))
            {
                mapLines.Add(new CodeLine(1, "builder.ToTable(\"{0}\");", table.Name));
            }
            else
            {
                mapLines.Add(new CodeLine(1, "builder.ToTable(\"{0}\", \"{1}\");", table.Name, table.Schema));
            }

            mapLines.Add(new CodeLine());

            var columns = default(List<Column>);

            if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
            {
                mapLines.Add(new CodeLine(1, "builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.Columns.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item.Name))))));
                mapLines.Add(new CodeLine());
            }
            else
            {
                mapLines.Add(new CommentLine(1, " Set key for entity"));

                if (table.PrimaryKey.Key.Count == 1)
                {
                    mapLines.Add(new CodeLine(1, "builder.HasKey(p => p.{0});", classDefinition.NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                    mapLines.Add(new CodeLine());
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    mapLines.Add(new CodeLine(1, "builder.HasKey(p => new {{ {0} }});", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table.Identity != null)
            {
                mapLines.Add(new CommentLine(1, " Set identity for entity (auto increment)"));
                mapLines.Add(new CodeLine(1, "builder.Property(p => p.{0}).UseSqlServerIdentityColumn();", classDefinition.NamingConvention.GetPropertyName(table.Identity.Name)));
                mapLines.Add(new CodeLine());
            }

            columns = table.Columns;

            mapLines.Add(new CommentLine(1, " Set configuration for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var lines = new List<string>()
                {
                    string.Format("builder.Property(p => p.{0})" , column.GetPropertyName())
                };

                if (string.Compare(column.Name, column.GetPropertyName()) != 0)
                {
                    lines.Add(string.Format("HasColumnName(\"{0}\")", column.Name));
                }

                if (column.IsString())
                {
                    lines.Add(column.Length == 0 ? string.Format("HasColumnType(\"{0}(max)\")", column.Type) : string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                }
                else if (column.IsDecimal())
                {
                    lines.Add(string.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                }
                else if (column.IsDouble() || column.IsSingle())
                {
                    lines.Add(string.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                }
                else
                {
                    lines.Add(string.Format("HasColumnType(\"{0}\")", column.Type));
                }

                if (!column.Nullable)
                {
                    lines.Add("IsRequired()");
                }

                mapLines.Add(new CodeLine(1, "{0};", string.Join(".", lines)));
            }

            mapLines.Add(new CodeLine());

            if (columns != null)
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];

                    if (!string.IsNullOrEmpty(project.Settings.ConcurrencyToken) && string.Compare(column.Name, project.Settings.ConcurrencyToken) == 0)
                    {
                        mapLines.Add(new CommentLine(1, " Set concurrency token for entity"));
                        mapLines.Add(new CodeLine(1, "builder"));
                        mapLines.Add(new CodeLine(2, ".Property(p => p.{0})", column.GetPropertyName()));
                        mapLines.Add(new CodeLine(2, ".ValueGeneratedOnAddOrUpdate()"));
                        mapLines.Add(new CodeLine(2, ".IsConcurrencyToken();"));
                        mapLines.Add(new CodeLine());
                    }
                }
            }

            if (table.Uniques.Count > 0)
            {
                mapLines.Add(new CommentLine(1, " Add configuration for uniques"));

                foreach (var unique in table.Uniques)
                {
                    mapLines.Add(new CodeLine(1, "builder"));

                    if (unique.Key.Count == 1)
                    {
                        mapLines.Add(new CodeLine(2, ".HasIndex(p => p.{0})", classDefinition.NamingConvention.GetPropertyName(unique.Key.First())));
                        mapLines.Add(new CodeLine(2, ".IsUnique()"));
                    }
                    else
                    {
                        mapLines.Add(new CodeLine(2, ".HasIndex(p => new {{ {0} }})", string.Join(", ", table.PrimaryKey.Key.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item))))));
                        mapLines.Add(new CodeLine(2, ".IsUnique()"));
                    }

                    mapLines.Add(new CodeLine(2, ".HasName(\"{0}\");", unique.ConstraintName));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table.ForeignKeys.Count > 0)
            {
                mapLines.Add(new CommentLine(1, " Add configuration for foreign keys"));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    if (foreignKey.Key.Count == 0)
                    {
                        continue;
                    }
                    else if (foreignKey.Key.Count == 1)
                    {
                        var foreignProperty = foreignKey.GetParentNavigationProperty(project, foreignTable);

                        mapLines.Add(new CodeLine(1, "builder"));
                        mapLines.Add(new CodeLine(2, ".HasOne(p => p.{0})", foreignProperty.Name));
                        mapLines.Add(new CodeLine(2, ".WithMany(b => b.{0})", table.GetPluralName()));
                        mapLines.Add(new CodeLine(2, ".HasForeignKey(p => {0})", string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(foreignKey.Key[0]))));
                        mapLines.Add(new CodeLine(2, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        mapLines.Add(new CodeLine());
                    }
                    else
                    {
                        // todo: add logic for key with multiple columns
                    }
                }
            }

            mapLines.Add(new CodeLine("});"));

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = mapLines
            };

            classDefinition.Methods.Add(mapMethod);

            return classDefinition;
        }

        public static CSharpClassDefinition GetEntityTypeConfigurationClassDefinition(this IView view, EntityFrameworkCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            if (project.Settings.UseMefForEntitiesMapping)
            {
                classDefinition.Namespaces.Add("System.Composition");

                classDefinition.Attributes.Add(new MetadataAttribute("Export", "typeof(IEntityTypeConfiguration)"));
            }

            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespaces.AddUnique(project.GetEntityLayerNamespace(view.HasDefaultSchema() ? string.Empty : view.Schema));

            classDefinition.Namespace = project.GetDataLayerMappingNamespace();

            classDefinition.Name = view.GetEntityTypeConfigurationName();

            classDefinition.Implements.Add("IEntityTypeConfiguration");

            var mapLines = new List<ILine>();

            mapLines.Add(new CodeLine("modelBuilder.Entity<{0}>(builder =>", view.GetSingularName()));
            mapLines.Add(new CodeLine("{"));

            mapLines.Add(new CommentLine(1, " Set configuration for entity"));

            if (string.IsNullOrEmpty(view.Schema))
            {
                mapLines.Add(new CodeLine(1, "builder.ToTable(\"{0}\");", view.Name));
            }
            else
            {
                mapLines.Add(new CodeLine(1, "builder.ToTable(\"{0}\", \"{1}\");", view.Name, view.Schema));
            }

            mapLines.Add(new CodeLine());

            var columns = view.Columns;

            mapLines.Add(new CodeLine(1, "builder.HasKey(p => new {{ {0} }});", string.Join(", ", columns.Select(item => string.Format("p.{0}", classDefinition.NamingConvention.GetPropertyName(item.Name))))));
            mapLines.Add(new CodeLine());

            mapLines.Add(new CommentLine(1, " Set configuration for columns"));

            mapLines.Add(new CodeLine("});"));

            var mapMethod = new MethodDefinition("void", "Configure", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = mapLines
            };

            classDefinition.Methods.Add(mapMethod);

            return classDefinition;
        }
    }
}
