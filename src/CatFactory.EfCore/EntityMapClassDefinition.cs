using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityMapClassDefinition : CSharpClassDefinition
    {
        public EntityMapClassDefinition(IDbObject mappedObject, EfCoreProject project)
        {
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = mappedObject.GetMapName();

            Implements.Add("IEntityMap");

            var mapMethodLines = new List<ILine>();

            mapMethodLines.Add(new CodeLine("var entity = modelBuilder.Entity<{0}>();", mappedObject.GetSingularName()));
            mapMethodLines.Add(new CodeLine());

            if (String.IsNullOrEmpty(mappedObject.Schema))
            {
                mapMethodLines.Add(new CodeLine("entity.ToTable(\"{0}\");", mappedObject.Name));
            }
            else
            {
                mapMethodLines.Add(new CodeLine("entity.ToTable(\"{0}\", \"{1}\");", mappedObject.Name, mappedObject.Schema));
            }

            mapMethodLines.Add(new CodeLine());

            var columns = default(List<Column>);

            var table = mappedObject as ITable;

            if (table != null)
            {
                if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
                {
                    mapMethodLines.Add(new CodeLine("entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item.Name))))));
                    mapMethodLines.Add(new CodeLine());
                }
                else
                {
                    if (table.PrimaryKey.Key.Count == 1)
                    {
                        mapMethodLines.Add(new CodeLine("entity.HasKey(p => p.{0});", NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                        mapMethodLines.Add(new CodeLine());
                    }
                    else if (table.PrimaryKey.Key.Count > 1)
                    {
                        mapMethodLines.Add(new CodeLine("entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                        mapMethodLines.Add(new CodeLine());
                    }
                }

                if (table.Identity != null)
                {
                    mapMethodLines.Add(new CodeLine("entity.Property(p => p.{0}).UseSqlServerIdentityColumn();", NamingConvention.GetPropertyName(table.Identity.Name)));
                    mapMethodLines.Add(new CodeLine());
                }

                if (table != null)
                {
                    foreach (var foreignKey in table.ForeignKeys)
                    {
                        if (foreignKey.Key.Count == 1)
                        {
                            var foreignTable = project.Database.Tables.FirstOrDefault(item => item.FullName == foreignKey.References);

                            if (foreignTable == null)
                            {
                                continue;
                            }

                            var foreignProperty = foreignKey.GetParentNavigationProperty(project, foreignTable);

                            mapMethodLines.Add(new CodeLine("entity"));
                            mapMethodLines.Add(new CodeLine(1, ".HasOne(p => p.{0})", foreignProperty.Name));
                            mapMethodLines.Add(new CodeLine(1, ".WithMany(b => b.{0})", table.GetPluralName()));
                            mapMethodLines.Add(new CodeLine(1, ".HasForeignKey(p => {0})", String.Format("p.{0}", NamingConvention.GetPropertyName(foreignKey.Key[0]))));
                            mapMethodLines.Add(new CodeLine(1, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                            mapMethodLines.Add(new CodeLine());
                        }
                    }

                    foreach (var unique in table.Uniques)
                    {
                        if (unique.Key.Count == 1)
                        {
                            mapMethodLines.Add(new CodeLine("entity"));
                            mapMethodLines.Add(new CodeLine(1, ".HasAlternateKey(p => new {{ {0} }})", String.Join(", ", unique.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                            mapMethodLines.Add(new CodeLine(1, ".HasName(\"{0}\");", unique.ConstraintName));
                            mapMethodLines.Add(new CodeLine());
                        }
                    }
                }

                columns = table.GetColumnsWithOutKey().ToList();
            }

            var view = mappedObject as IView;

            if (view != null)
            {
                columns = view.Columns;
            }

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!String.IsNullOrEmpty(project.Settings.ConcurrencyToken) && column.Name == project.Settings.ConcurrencyToken)
                {
                    mapMethodLines.Add(new CodeLine("entity"));
                    mapMethodLines.Add(new CodeLine(1, ".Property(p => p.{0})", column.GetPropertyName()));
                    mapMethodLines.Add(new CodeLine(1, ".ValueGeneratedOnAddOrUpdate()"));
                    mapMethodLines.Add(new CodeLine(1, ".IsConcurrencyToken();"));
                }
                else
                {
                    var lines = new List<String>()
                    {
                        String.Format("entity.Property(p => p.{0})", column.GetPropertyName())
                    };

                    if (String.Compare(column.Name, column.GetPropertyName()) != 0)
                    {
                        lines.Add(String.Format("HasColumnName(\"{0}\")", column.Name));
                    }

                    switch (column.Type)
                    {
                        case "char":
                        case "nchar":
                        case "varchar":
                        case "nvarchar":
                            lines.Add(column.Length == 0 ? String.Format("HasColumnType(\"{0}(max)\")", column.Type) : String.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                            break;

                        case "decimal":
                            lines.Add(String.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                            break;

                        default:
                            lines.Add(String.Format("HasColumnType(\"{0}\")", column.Type));
                            break;
                    }

                    if (!column.Nullable)
                    {
                        lines.Add("IsRequired()");
                    }

                    mapMethodLines.Add(new CodeLine("{0};", String.Join(".", lines)));

                    if (i < columns.Count - 1)
                    {
                        mapMethodLines.Add(new CodeLine());
                    }
                }
            }
            
            var mapMethod = new MethodDefinition("void", "Map", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = mapMethodLines
            };

            Methods.Add(mapMethod);
        }
    }
}
