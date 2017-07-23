using System;
using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EntityLayerExtensions
    {
        private static void GenerateEntityInterface(EfCoreProject project)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = new EntityInterfaceDefinition
                {
                    Namespace = project.GetEntityLayerNamespace()
                },
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetEntityLayerDirectory());

            if (project.Settings.AuditEntity != null)
            {
                codeBuilder.ObjectDefinition = new AuditEntityInterfaceDefinition(project)
                {
                    Namespace = project.GetEntityLayerNamespace(),
                };

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }
        }

        public static EfCoreProject GenerateEntityLayer(this EfCoreProject project)
        {
            GenerateEntityInterface(project);

            foreach (var table in project.Database.Tables)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = new EntityClassDefinition(table, project)
                    {
                        Namespace = project.GetEntityLayerNamespace(),
                    },
                    OutputDirectory = project.OutputDirectory
                };

                if (project.Settings.UseDataAnnotations)
                {
                    codeBuilder.ObjectDefinition.Attributes.Add(new MetadataAttribute("Table", String.Format("\"{0}\"", table.Name))
                    {
                        Sets = new List<MetadataAttributeSet>()
                        {
                            new MetadataAttributeSet("Schema", String.Format("\"{0}\"", table.Schema))
                        }
                    });

                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        var column = table.Columns[i];

                        foreach (var property in codeBuilder.ObjectDefinition.Properties)
                        {
                            if (column.GetPropertyName() == property.Name)
                            {
                                if (table.Identity != null && table.Identity.Name == column.Name)
                                {
                                    property.Attributes.Add(new MetadataAttribute("DatabaseGenerated", "DatabaseGeneratedOption.Identity"));
                                }

                                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                                {
                                    property.Attributes.Add(new MetadataAttribute("Key"));
                                }

                                property.Attributes.Add(new MetadataAttribute("Column", String.Format("Order = {0}", i + 1)));

                                if (!column.Nullable)
                                {
                                    property.Attributes.Add(new MetadataAttribute("Required"));
                                }

                                if (column.Type.Contains("char") && column.Length > 0)
                                {
                                    property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));
                                }
                            }
                        }
                    }
                }

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            foreach (var view in project.Database.Views)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = new EntityClassDefinition(view, project)
                    {
                        Namespace = project.GetEntityLayerNamespace()
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            return project;
        }
    }
}
