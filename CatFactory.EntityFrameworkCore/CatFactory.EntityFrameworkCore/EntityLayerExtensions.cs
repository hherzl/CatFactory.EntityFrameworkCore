using CatFactory.NetCore;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityLayerExtensions
    {
        private static void ScaffoldEntityInterface(EntityFrameworkCoreProject project)
        {
            var selection = project.GlobalSelection();

            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, project.GetEntityInterfaceDefinition());

            if (selection.Settings.AuditEntity != null)
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, project.GetAuditEntityInterfaceDefinition());
        }

        public static EntityFrameworkCoreProject ScaffoldEntityLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldEntityInterface(project);

            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                var classDefinition = project.GetEntityClassDefinition(table);

                if (selection.Settings.UseDataAnnotations)
                    classDefinition.AddDataAnnotations(table, project);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var classDefinition = project.GetEntityClassDefinition(view);

                if (selection.Settings.UseDataAnnotations)
                    classDefinition.AddDataAnnotations(view, project);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, classDefinition);
            }

            return project;
        }
    }
}
