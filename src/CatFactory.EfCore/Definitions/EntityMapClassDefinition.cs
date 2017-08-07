using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class EntityMapClassDefinition : CSharpClassDefinition
    {
        public EntityMapClassDefinition(IDbObject mappedObject, EfCoreProject project)
            : base()
        {
            MappedObject = mappedObject;
            Project = project;

            Init();
        }

        public IDbObject MappedObject { get; }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            if (Project.Settings.UseMefForEntitiesMapping)
            {
                Namespaces.Add("System.Composition");

                Attributes.Add(new MetadataAttribute("Export", "typeof(IEntityMap)"));
            }

            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Namespaces.AddUnique(Project.GetEntityLayerNamespace(MappedObject.HasDefaultSchema() ? String.Empty : MappedObject.Schema));

            Namespace = Project.GetDataLayerMappingNamespace();

            Name = MappedObject.GetMapName();

            Implements.Add("IEntityMap");

            var mapLines = new List<ILine>();

            mapLines.Add(new CodeLine("modelBuilder.Entity<{0}>(entity =>", MappedObject.GetSingularName()));
            mapLines.Add(new CodeLine("{{"));

            mapLines.Add(new CommentLine(1, " Mapping for table"));

            if (String.IsNullOrEmpty(MappedObject.Schema))
            {
                mapLines.Add(new CodeLine(1, "entity.ToTable(\"{0}\");", MappedObject.Name));
            }
            else
            {
                mapLines.Add(new CodeLine(1, "entity.ToTable(\"{0}\", \"{1}\");", MappedObject.Name, MappedObject.Schema));
            }

            mapLines.Add(new CodeLine());

            var columns = default(List<Column>);

            var table = MappedObject as ITable;

            if (table != null)
            {
                if (table.PrimaryKey == null || table.PrimaryKey.Key.Count == 0)
                {
                    mapLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.Columns.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item.Name))))));
                    mapLines.Add(new CodeLine());
                }
                else
                {
                    mapLines.Add(new CommentLine(1, " Set key for entity"));

                    if (table.PrimaryKey.Key.Count == 1)
                    {
                        mapLines.Add(new CodeLine(1, "entity.HasKey(p => p.{0});", NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                        mapLines.Add(new CodeLine());
                    }
                    else if (table.PrimaryKey.Key.Count > 1)
                    {
                        mapLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                        mapLines.Add(new CodeLine());
                    }
                }

                if (table.Identity != null)
                {
                    mapLines.Add(new CommentLine(1, " Set identity for entity (auto increment)"));
                    mapLines.Add(new CodeLine(1, "entity.Property(p => p.{0}).UseSqlServerIdentityColumn();", NamingConvention.GetPropertyName(table.Identity.Name)));
                    mapLines.Add(new CodeLine());
                }

                columns = table.Columns;
            }

            var view = MappedObject as IView;

            if (view != null)
            {
                columns = view.Columns;

                mapLines.Add(new CodeLine(1, "entity.HasKey(p => new {{ {0} }});", String.Join(", ", columns.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item.Name))))));
                mapLines.Add(new CodeLine());
            }

            mapLines.Add(new CommentLine(1, " Set mapping for columns"));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var lines = new List<String>()
                {
                    String.Format("entity.Property(p => p.{0})" , column.GetPropertyName())
                };

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

                mapLines.Add(new CodeLine(1, "{0};", String.Join(".", lines)));
            }

            mapLines.Add(new CodeLine());
            
            if (columns != null)
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];

                    if (!String.IsNullOrEmpty(Project.Settings.ConcurrencyToken) && String.Compare(column.Name, Project.Settings.ConcurrencyToken) == 0)
                    {
                        mapLines.Add(new CommentLine(1, " Set concurrency token for entity"));
                        mapLines.Add(new CodeLine(1, "entity"));
                        mapLines.Add(new CodeLine(2, ".Property(p => p.{0})", column.GetPropertyName()));
                        mapLines.Add(new CodeLine(2, ".ValueGeneratedOnAddOrUpdate()"));
                        mapLines.Add(new CodeLine(2, ".IsConcurrencyToken();"));
                        mapLines.Add(new CodeLine());
                    }
                }
            }
            
            if (table != null && table.Uniques.Count > 0)
            {
                mapLines.Add(new CommentLine(1, " Add configuration for uniques"));

                foreach (var unique in table.Uniques)
                {
                    mapLines.Add(new CodeLine(1, "entity"));

                    if (unique.Key.Count == 1)
                    {
                        mapLines.Add(new CodeLine(2, ".HasAlternateKey(p => new {{ {0} }})", String.Join(", ", unique.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                    }
                    else
                    {
                        mapLines.Add(new CodeLine(2, ".HasAlternateKey(p => new {{ {0} }})", String.Join(", ", table.PrimaryKey.Key.Select(item => String.Format("p.{0}", NamingConvention.GetPropertyName(item))))));
                    }

                    mapLines.Add(new CodeLine(2, ".HasName(\"{0}\");", unique.ConstraintName));
                    mapLines.Add(new CodeLine());
                }
            }

            if (table != null && table.ForeignKeys.Count > 0)
            {
                mapLines.Add(new CommentLine(1, " Add configuration for foreign keys"));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = Project.Database.FindTableByFullName(foreignKey.References);

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
                        var foreignProperty = foreignKey.GetParentNavigationProperty(Project, foreignTable);

                        mapLines.Add(new CodeLine(1, "entity"));
                        mapLines.Add(new CodeLine(2, ".HasOne(p => p.{0})", foreignProperty.Name));
                        mapLines.Add(new CodeLine(2, ".WithMany(b => b.{0})", table.GetPluralName()));
                        mapLines.Add(new CodeLine(2, ".HasForeignKey(p => {0})", String.Format("p.{0}", NamingConvention.GetPropertyName(foreignKey.Key[0]))));
                        mapLines.Add(new CodeLine(2, ".HasConstraintName(\"{0}\");", foreignKey.ConstraintName));
                        mapLines.Add(new CodeLine());
                    }
                    else
                    {
                        // todo: add logic for key with multiple columns
                    }
                }
            }

            mapLines.Add(new CodeLine("}});"));

            var mapMethod = new MethodDefinition("void", "Map", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = mapLines
            };

            Methods.Add(mapMethod);
        }
    }
}
