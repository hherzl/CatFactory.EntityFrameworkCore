using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class MappingClassDefinition : CSharpClassDefinition
    {
        public MappingClassDefinition(IDbObject mappedObject)
        {
            Namespaces = new List<String>()
            {
                "Microsoft.EntityFrameworkCore"
            };

            Name = mappedObject.GetMapName();

            Implements.Add("IEntityMap");

            var mapMethodLines = new List<CodeLine>();

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
                if (table.PrimaryKey == null)
                {
                    mapMethodLines.Add(new CodeLine("entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", item.Name)))));
                    mapMethodLines.Add(new CodeLine());
                }
                else
                {
                    if (table.PrimaryKey.Key.Count == 0)
                    {
                        mapMethodLines.Add(new CodeLine("entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", item)))));
                        mapMethodLines.Add(new CodeLine());
                    }
                    else if (table.PrimaryKey.Key.Count == 1)
                    {
                        mapMethodLines.Add(new CodeLine("entity.HasKey(p => p.{0});", table.PrimaryKey.Key[0]));
                        mapMethodLines.Add(new CodeLine());
                    }
                    else if (table.PrimaryKey.Key.Count > 1)
                    {
                        mapMethodLines.Add(new CodeLine("entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", item)))));
                        mapMethodLines.Add(new CodeLine());
                    }
                }

                if (table.Identity != null)
                {
                    mapMethodLines.Add(new CodeLine("entity.Property(p => p.{0}).UseSqlServerIdentityColumn();", table.Identity.Name));
                    mapMethodLines.Add(new CodeLine());
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

            var mapMethod = new MethodDefinition("void", "Map")
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("ModelBuilder", "modelBuilder")
                },
                Lines = mapMethodLines
            };

            Methods.Add(mapMethod);
        }
    }
}
