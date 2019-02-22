using System.Collections.Generic;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityLayerExtensions
    {
        public static EntityFrameworkCoreProject ScaffoldEntityLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldEntityInterface(project);
            ScaffoldEntities(project);

            return project;
        }

        private static void ScaffoldEntityInterface(EntityFrameworkCoreProject project)
        {
            var selection = project.GlobalSelection();

            project.ObjectDefinitions = new List<IObjectDefinition>
            {
                project.GetEntityInterfaceDefinition(),
                project.GetAuditEntityInterfaceDefinition()
            };

            //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, project.GetEntityInterfaceDefinition());

            //if (selection.Settings.AuditEntity != null)
            //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, project.GetAuditEntityInterfaceDefinition());
        }

        private static EntityFrameworkCoreProject ScaffoldEntities(this EntityFrameworkCoreProject project)
        {
            project.ObjectDefinitions = new List<IObjectDefinition>();

            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                var definition = project.GetEntityClassDefinition(table);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(table, project);

                project.ObjectDefinitions.Add(definition);

                //var selection = project.GetSelection(table);

                //var definition = project.GetEntityClassDefinition(table);

                //if (selection.Settings.UseDataAnnotations)
                //    definition.AddDataAnnotations(table, project);

                //if (project.Database.HasDefaultSchema(table))
                //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
                //else
                //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(table.Schema), selection.Settings.ForceOverwrite, definition);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var definition = project.GetEntityClassDefinition(view);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(view, project);

                project.ObjectDefinitions.Add(definition);

                //var selection = project.GetSelection(table);

                //var definition = project.GetEntityClassDefinition(table);

                //if (selection.Settings.UseDataAnnotations)
                //    definition.AddDataAnnotations(table, project);

                //if (project.Database.HasDefaultSchema(table))
                //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
                //else
                //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(table.Schema), selection.Settings.ForceOverwrite, definition);
            }

            project.Scaffold();

            //foreach (var table in project.Database.Tables)
            //{
            //    var selection = project.GetSelection(table);

            //    var definition = project.GetEntityClassDefinition(table);

            //    if (selection.Settings.UseDataAnnotations)
            //        definition.AddDataAnnotations(table, project);

            //    if (project.Database.HasDefaultSchema(table))
            //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
            //    else
            //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(table.Schema), selection.Settings.ForceOverwrite, definition);
            //}

            //foreach (var view in project.Database.Views)
            //{
            //    var selection = project.GetSelection(view);

            //    var definition = project.GetEntityClassDefinition(view);

            //    if (selection.Settings.UseDataAnnotations)
            //        definition.AddDataAnnotations(view, project);

            //    if (project.Database.HasDefaultSchema(view))
            //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, definition);
            //    else
            //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(view.Schema), selection.Settings.ForceOverwrite, definition);
            //}

            return project;
        }
    }
}
