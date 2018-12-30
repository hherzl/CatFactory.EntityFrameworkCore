using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;

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

                var definition = project.GetEntityClassDefinition(table);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(table, project);

                if (project.Database.HasDefaultSchema(table))
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
                else
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(table.Schema), selection.Settings.ForceOverwrite, definition);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var definition = project.GetEntityClassDefinition(view);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(view, project);

                if (project.Database.HasDefaultSchema(view))
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
                else
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(view.Schema), selection.Settings.ForceOverwrite, definition);
            }

            return project;
        }
    }
}
