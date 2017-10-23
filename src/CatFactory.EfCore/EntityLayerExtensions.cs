using CatFactory.DotNetCore;
using CatFactory.EfCore.Definitions;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EntityLayerExtensions
    {
        private static void GenerateEntityInterface(EfCoreProject project)
        {
            CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetEntityInterfaceDefinition());

            if (project.Settings.AuditEntity != null)
            {
                CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetAuditEntityInterfaceDefinition());
            }
        }

        public static EfCoreProject GenerateEntityLayer(this EfCoreProject project)
        {
            GenerateEntityInterface(project);

            foreach (var table in project.Database.Tables)
            {
                var classDefinition = table.GetEntityClassDefinition(project);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddTableAttribute(table);

                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        var column = table.Columns[i];

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

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var view in project.Database.Views)
            {
                var classDefinition = view.GetEntityClassDefinition(project);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddTableAttribute(view);

                    for (var i = 0; i < view.Columns.Count; i++)
                    {
                        var column = view.Columns[i];

                        foreach (var property in classDefinition.Properties)
                        {
                            if (column.GetPropertyName() == property.Name)
                            {
                                property.Attributes.Add(new MetadataAttribute("Key"));
                                property.Attributes.Add(new MetadataAttribute("Column", string.Format("Order = {0}", i + 1)));
                            }
                        }
                    }
                }

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            return project;
        }
    }
}
