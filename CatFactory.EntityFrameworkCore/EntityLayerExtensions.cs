using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

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
            project.Scaffold(project.GetEntityInterfaceDefinition(false), project.GetEntityLayerDirectory());

            if (project.GlobalSelection().Settings.AuditEntity != null)
                project.Scaffold(project.GetAuditEntityInterfaceDefinition(false), project.GetEntityLayerDirectory());
        }

        private static EntityFrameworkCoreProject ScaffoldEntities(this EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                var definition = project.GetEntityClassDefinition(table, false);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(table, project);

                project.Scaffold(definition, project.GetEntityLayerDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var definition = project.GetEntityClassDefinition(view, project.Database.HasDefaultSchema(view) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema));

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(view, project);

                project.Scaffold(definition, project.GetEntityLayerDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
            }

            // todo: Review this scaffolding

            //foreach (var tableFunction in project.Database.TableFunctions)
            //{
            //    var selection = project.GetSelection(tableFunction);

            //    var definition = project.GetEntityClassDefinition(tableFunction);

            //    project.Scaffold(definition, project.GetEntityLayerDirectory(), project.Database.HasDefaultSchema(tableFunction) ? "" : tableFunction.Schema);
            //}

            // todo: Review this scaffolding

            //foreach (var storedProcedure in project.Database.StoredProcedures)
            //{
            //    var selection = project.GetSelection(storedProcedure);

            //    var definition = project.GetEntityClassDefinition(storedProcedure);

            //    project.Scaffold(definition, project.GetEntityLayerDirectory(), project.Database.HasDefaultSchema(storedProcedure) ? "" : storedProcedure.Schema);
            //}

            return project;
        }
    }
}
