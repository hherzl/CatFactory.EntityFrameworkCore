using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class InfrastructureExtensions
{
    public static EntityFrameworkCoreProject ScaffoldInfrastructure(this EntityFrameworkCoreProject project, Action<InfrastructureSettings> action = null)
    {
        ScaffoldInfrastructureConfigureServices(project);
        ScaffoldConfigurations(project);
        ScaffoldDbContext(project);

        return project;
    }

    internal static void ScaffoldInfrastructureConfigureServices(EntityFrameworkCoreProject project)
    {
        project.Scaffold(project.GetInfrastructureConfigureServicesClassDefinition(), project.GetInfrastructureDirectory());
    }

    internal static void ScaffoldConfigurations(EntityFrameworkCoreProject project)
    {
        var projectSelection = project.GlobalSelection();

        if (projectSelection.Settings.UseDataAnnotations)
            return;

        foreach (var table in project.Database.Tables)
        {
            var definition = project.GetEntityConfigurationClassDefinition(table, true);
            project.Scaffold(definition, project.GetInfrastructureConfigurationsDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
        }

        foreach (var view in project.Database.Views)
        {
            var definition = project.GetEntityConfigurationClassDefinition(view, false);
            project.Scaffold(definition, project.GetInfrastructureConfigurationsDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
        }
    }

    internal static void ScaffoldDbContext(EntityFrameworkCoreProject project)
    {
        var projectSelection = project.GlobalSelection();
        project.Scaffold(project.GetDbContextClassDefinition(projectSelection, true), project.GetInfrastructurePersistenceDirectory());
    }
}
