using CatFactory.DotNetCore;
using CatFactory.EfCore.Definitions;

namespace CatFactory.EfCore
{
    public static class EntityLayerExtensions
    {
        private static void ScaffoldEntityInterface(EntityFrameworkCoreProject project)
        {
            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetEntityInterfaceDefinition());

            if (project.Settings.AuditEntity != null)
            {
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.GetAuditEntityInterfaceDefinition());
            }
        }

        public static EntityFrameworkCoreProject ScaffoldEntityLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldEntityInterface(project);

            foreach (var table in project.Database.Tables)
            {
                var classDefinition = project.GetEntityClassDefinition(table);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddDataAnnotations(table);
                }

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var view in project.Database.Views)
            {
                var classDefinition = project.GetEntityClassDefinition(view);

                if (project.Settings.UseDataAnnotations)
                {
                    classDefinition.AddDataAnnotations(view, project);
                }

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }

            return project;
        }
    }
}
