using System.Linq;
using CatFactory.DotNetCore;
using CatFactory.EfCore.Definitions;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EntityLayerExtensions
    {
        private static void GenerateEntityInterface(EntityFrameworkCoreProject project)
        {
            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetEntityInterfaceDefinition());

            if (project.Settings.AuditEntity != null)
            {
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetAuditEntityInterfaceDefinition());
            }
        }

        public static EntityFrameworkCoreProject ScaffoldEntityLayer(this EntityFrameworkCoreProject project)
        {
            GenerateEntityInterface(project);

            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetEntityClassDefinition(table);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddTableAttribute(table);

                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        var column = table[i];

                        foreach (var property in classDefinition.Properties)
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
                                else
                                {
                                    property.Attributes.Add(new MetadataAttribute("Column"));
                                }

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

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            var primaryKeys = project.Database.Tables.Where(item => item.PrimaryKey != null).Select(item => item.PrimaryKey?.GetColumns(item).Select(c => c.Name).First()).ToList();

            foreach (var view in project.Database.Views)
            {
                var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

                var classDefinition = project.GetEntityClassDefinition(view);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddTableAttribute(view);

                    for (var i = 0; i < view.Columns.Count; i++)
                    {
                        var column = view[i];

                        foreach (var property in classDefinition.Properties)
                        {
                            if (primaryKeys.Contains(column.Name))
                            {
                                property.Attributes.Add(new MetadataAttribute("Key"));
                            }

                            if (column.GetPropertyName() == property.Name)
                            {
                                property.Attributes.Add(new MetadataAttribute("Column", string.Format("Order = {0}", i + 1)));
                            }
                        }
                    }
                }

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            return project;
        }
    }
}
