using CatFactory.DotNetCore;
using CatFactory.EfCore.Definitions;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EntityLayerExtensions
    {
        private static void GenerateEntityInterface(EfCoreProject project)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = project.GetEntityInterfaceDefinition(),
                OutputDirectory = project.OutputDirectory,
                ForceOverwrite = project.Settings.ForceOverwrite
            };

            codeBuilder.CreateFile(project.GetEntityLayerDirectory());

            if (project.Settings.AuditEntity != null)
            {
                codeBuilder.ObjectDefinition = project.GetAuditEntityInterfaceDefinition();

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
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

                                property.Attributes.Add(new MetadataAttribute("Column", string.Format("Order = {0}", i + 1)));

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

                CSharpClassBuilder.Create(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var view in project.Database.Views)
            {
                CSharpClassBuilder.Create(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, view.GetEntityClassDefinition(project));
            }

            return project;
        }
    }
}
