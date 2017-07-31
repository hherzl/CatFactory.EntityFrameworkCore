using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityMapClassDefinition : CSharpClassDefinition
    {
        public EntityMapClassDefinition(IDbObject mappedObject, EfCoreProject project)
        {
            this.mappedObject = mappedObject;
            this.project = project;
            
            Init();
        }

        public IDbObject mappedObject { get; }

        public EfCoreProject project { get; }

        public override void Init()
        {
            if (project.Settings.UseMefForEntitiesMapping)
            {
                Namespaces.Add("System.Composition");

                Attributes.Add(new MetadataAttribute("Export", "typeof(IEntityMap)"));
            }

            Namespaces.Add("Microsoft.EntityFrameworkCore");

            if (mappedObject.HasDefaultSchema())
            {
                Namespaces.AddUnique(project.GetEntityLayerNamespace());
            }
            else
            {
                Namespaces.AddUnique(project.GetEntityLayerNamespace(mappedObject.Schema));
            }

            Namespace = project.GetDataLayerMappingNamespace();

            Name = mappedObject.GetMapName();

            Implements.Add("IEntityMap");

            var mapMethodLines = new List<ILine>();

            mapMethodLines.Add(new CodeLine("modelBuilder.Entity<{0}>(entity =>", mappedObject.GetSingularName()));
            mapMethodLines.Add(new CodeLine("{{"));

            if (String.IsNullOrEmpty(mappedObject.Schema))
            {
                mapMethodLines.Add(new CodeLine(1, "entity.ToTable(\"{0}\");", mappedObject.Name));
            }
            else
            {
                mapMethodLines.Add(new CodeLine(1, "entity.ToTable(\"{0}\", \"{1}\");", mappedObject.Name, mappedObject.Schema));
            }

            mapMethodLines.Add(new CodeLine());

            var columns = default(List<Column>);

            var table = mappedObject as ITable;

            if (table != null)
            {
                if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
                {
                    mapMethodLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item.Name))))));
                    mapMethodLines.Add(new CodeLine());
                }
                else
                {
                    if (table.PrimaryKey.Key.Count == 1)
                    {
                        mapMethodLines.Add(new CodeLine(1, "entity.HasKey(p => p.{0});", NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                        mapMethodLines.Add(new CodeLine());
                    }
                    else if (table.PrimaryKey.Key.Count > 1)
                    {
                        mapMethodLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                        mapMethodLines.Add(new CodeLine());
                    }
                }

                if (table.Identity != null)
                {
                    mapMethodLines.Add(new CodeLine(1, "entity.Property(p => p.{0}).UseSqlServerIdentityColumn();", NamingConvention.GetPropertyName(table.Identity.Name)));
                    mapMethodLines.Add(new CodeLine());
                }

                if (table != null)
                {
                    foreach (var fk in table.ForeignKeys)
                    {
                        var foreignTable = project.Database.FindTableByFullName(fk.References);

                        if (foreignTable == null)
                        {
                            continue;
                        }

                        if (fk.Key.Count == 0)
                        {
                            continue;
                        }
                        else if (fk.Key.Count == 1)
                        {
                            var foreignProperty = fk.GetParentNavigationProperty(project, foreignTable);

                            mapMethodLines.Add(new CodeLine(1, "entity"));
                            mapMethodLines.Add(new CodeLine(2, ".HasOne(p => p.{0})", foreignProperty.Name));
                            mapMethodLines.Add(new CodeLine(2, ".WithMany(b => b.{0})", table.GetPluralName()));
                            mapMethodLines.Add(new CodeLine(2, ".HasForeignKey(p => {0})", String.Format("p.{0}", NamingConvention.GetPropertyName(fk.Key[0]))));
                            mapMethodLines.Add(new CodeLine(2, ".HasConstraintName(\"{0}\");", fk.ConstraintName));
                            mapMethodLines.Add(new CodeLine());
                        }
                        else
                        {
                            // todo: add logic for key with multiple columns
                        }
                    }

                    foreach (var unique in table.Uniques)
                    {
                        mapMethodLines.Add(new CodeLine(1, "entity"));

                        if (unique.Key.Count == 1)
                        {
                            mapMethodLines.Add(new CodeLine(2, ".HasAlternateKey(p => new {{ {0} }})", String.Join(", ", unique.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                        }
                        else
                        {
                            mapMethodLines.Add(new CodeLine(2, ".HasAlternateKey(p => new {{ {0} }})", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                        }

                        mapMethodLines.Add(new CodeLine(2, ".HasName(\"{0}\");", unique.ConstraintName));
                        mapMethodLines.Add(new CodeLine());
                    }
                }

                columns = table.GetColumnsWithOutKey().ToList();
            }

            var view = mappedObject as IView;

            if (view != null)
            {
                columns = view.Columns;

                mapMethodLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", columns.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item.Name))))));
                mapMethodLines.Add(new CodeLine());
            }

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (!String.IsNullOrEmpty(project.Settings.ConcurrencyToken) && String.Compare(column.Name, project.Settings.ConcurrencyToken) == 0)
                {
                    mapMethodLines.Add(new CodeLine(1, "entity"));
                    mapMethodLines.Add(new CodeLine(2, ".Property(p => p.{0})", column.GetPropertyName()));
                    mapMethodLines.Add(new CodeLine(2, ".ValueGeneratedOnAddOrUpdate()"));
                    mapMethodLines.Add(new CodeLine(2, ".IsConcurrencyToken();"));
                }
                else
                {
                    var lines = new List<String>()
                    {
                        String.Format("entity.Property(p => p.{0})" , column.GetPropertyName())
                    };

                    if (table != null)
                    {
                        if (project.Settings.BackingFields.Contains(table.GetFullColumnName(column)))
                        {
                            lines.Add(String.Format("HasField(\"{0}\")", NamingConvention.GetFieldName(column.GetPropertyName())));
                        }
                    }

                    if (String.Compare(column.Name, column.GetPropertyName()) != 0)
                    {
                        lines.Add(String.Format("HasColumnName(\"{0}\")", column.Name));
                    }

                    if (column.IsString())
                    {
                        lines.Add(column.Length == 0 ? String.Format("HasColumnType(\"{0}(max)\")", column.Type) : String.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Length));
                    }
                    else if (column.IsDecimal())
                    {
                        lines.Add(String.Format("HasColumnType(\"{0}({1}, {2})\")", column.Type, column.Prec, column.Scale));
                    }
                    else if (column.IsDouble() || column.IsSingle())
                    {
                        lines.Add(String.Format("HasColumnType(\"{0}({1})\")", column.Type, column.Prec));
                    }
                    else
                    {
                        lines.Add(String.Format("HasColumnType(\"{0}\")", column.Type));
                    }

                    if (!column.Nullable)
                    {
                        lines.Add("IsRequired()");
                    }

                    mapMethodLines.Add(new CodeLine(1, "{0};", String.Join(".", lines)));

                    if (i < columns.Count - 1)
                    {
                        mapMethodLines.Add(new CodeLine());
                    }
                }
            }

            mapMethodLines.Add(new CodeLine("}});"));

            var mapMethod = new MethodDefinition("void", "Map", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = mapMethodLines
            };

            Methods.Add(mapMethod);
        }
    }
}
