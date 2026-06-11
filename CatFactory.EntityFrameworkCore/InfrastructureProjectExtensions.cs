using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class InfrastructureProjectExtensions
{
    #region [ Directory naming ]

    public static string GetInfrastructureDirectory(this InfrastructureProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Solution.Namespaces.Infrastructure}");

    public static string GetInfrastructurePersistenceDirectory(this InfrastructureProject project)
        => Path.Combine(project.GetInfrastructureDirectory(), project.Solution.Namespaces.Persistence);

    public static string GetInfrastructureConfigurationsDirectory(this InfrastructureProject project)
        => Path.Combine(project.GetInfrastructurePersistenceDirectory(), project.Solution.Namespaces.Configurations);

    #endregion

    #region [ Namespaces ]

    public static string GetInfrastructureNs(this InfrastructureProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.Solution.Namespaces.Infrastructure);

    public static string GetInfrastructurePersistenceNs(this InfrastructureProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetInfrastructureNs(), project.Solution.Namespaces.Persistence);

    public static string GetInfrastructurePersistenceConfigurationsNs(this InfrastructureProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetInfrastructurePersistenceNs(), project.Solution.Namespaces.Configurations);

    #endregion

    #region [ Naming convention ]

    public static string GetDbContextName(this InfrastructureProject project, Database database)
        => project.CodeNamingConvention.GetClassName(string.Format("{0}DbContext", database.Name));

    public static string GetDbSetPropName(this InfrastructureProject project, IDbObject dbObject, bool pluralize)
        => pluralize ? project.NamingService.Pluralize(project.Solution.Domain.GetEntityName(dbObject)) : project.Solution.Domain.GetEntityName(dbObject);

    public static string GetFullDbSetPropName(this InfrastructureProject project, IDbObject dbObject)
        => project.NamingService.Pluralize(string.Concat(project.CodeNamingConvention.GetNamespace(dbObject.Schema), project.Solution.Domain.GetEntityName(dbObject)));

    public static string GetEntityConfigurationName(this InfrastructureProject project, IDbObject dbObject)
        => string.Format("{0}Configuration", project.Solution.Domain.GetEntityName(dbObject));

    public static string GetFullEntityConfigurationName(this InfrastructureProject project, IDbObject dbObject)
        => project.CodeNamingConvention.GetNamespace(project.GetInfrastructurePersistenceConfigurationsNs(), project.GetEntityConfigurationName(dbObject));

    #endregion

    public static InfrastructureProject Scaffold(this InfrastructureProject project, Action<InfrastructureSettings> action = null)
    {
        ScaffoldInfrastructureConfigureServices(project);
        ScaffoldConfigurations(project);
        ScaffoldDbContext(project);

        return project;
    }

    internal static void ScaffoldInfrastructureConfigureServices(InfrastructureProject project)
    {
        project.Scaffold(project.GetInfrastructureConfigureServicesClassDefinition(), project.GetInfrastructureDirectory());
    }

    internal static void ScaffoldConfigurations(InfrastructureProject project)
    {
        var projectSelection = project.GlobalSelection();

        if (projectSelection.Settings.UseDataAnnotations)
            return;

        foreach (var table in project.Database.Tables)
        {
            var definition = project.GetEntityConfigurationClassDef(table);
            project.Scaffold(definition, project.GetInfrastructureConfigurationsDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
        }

        foreach (var view in project.Database.Views)
        {
            var definition = project.GetEntityConfigurationClassDef(view);
            project.Scaffold(definition, project.GetInfrastructureConfigurationsDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
        }
    }

    internal static void ScaffoldDbContext(InfrastructureProject project)
    {
        var projectSelection = project.GlobalSelection();
        project.Scaffold(project.GetDbContextClassDef(projectSelection), project.GetInfrastructurePersistenceDirectory());
    }
}
