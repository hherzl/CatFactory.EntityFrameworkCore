using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class ApplicationProjectExtensions
{
    #region [ Directory naming ]

    public static string GetApplicationDirectory(this ApplicationProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Solution.Namespaces.Application}");

    public static string GetApplicationCommonDirectory(this ApplicationProject project)
        => Path.Combine(project.GetApplicationDirectory(), project.Solution.Namespaces.Common);

    public static string GetApplicationFeaturesDirectory(this ApplicationProject project)
        => Path.Combine(project.GetApplicationDirectory(), project.Solution.Namespaces.Features);

    public static string GetApplicationFeaturesDirectory(this ApplicationProject project, string feature)
        => Path.Combine(project.GetApplicationDirectory(), project.Solution.Namespaces.Features, feature);

    public static string GetApplicationQuerySpecsDirectory(this ApplicationProject project)
        => Path.Combine(project.GetApplicationDirectory(), project.Solution.Namespaces.QuerySpecs);

    #endregion

    #region [ Namespaces ]

    public static string GetApplicationNs(this ApplicationProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.Solution.Namespaces.Application);

    public static string GetApplicationCommonNs(this ApplicationProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.Solution.Namespaces.Common);

    public static string GetApplicationFeaturesNs(this ApplicationProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.Solution.Namespaces.Features);

    public static string GetApplicationFeaturesNs(this ApplicationProject project, string feature)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationFeaturesNs(), feature);

    public static string GetApplicationQuerySpecsNs(this ApplicationProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.Solution.Namespaces.QuerySpecs);

    #endregion

    #region [ Naming convention ]

    public static string GetDbContextInterfaceName(this ApplicationProject project, Database database)
        => project.CodeNamingConvention.GetClassName(string.Format("I{0}DbContext", database.Name));

    public static string GetCommandHandlerClassName(this ApplicationProject project, string feature)
        => project.CodeNamingConvention.GetClassName($"{feature}CommandHandler");

    public static string GetQueryHandlerClassName(this ApplicationProject project, string feature)
        => project.CodeNamingConvention.GetClassName($"{feature}QueryHandler");

    public static string GetQueryClassName(this ApplicationProject project, string feature)
        => project.CodeNamingConvention.GetClassName($"{feature}Query");

    public static string GetCommandClassName(this ApplicationProject project, string feature)
        => project.CodeNamingConvention.GetClassName($"{feature}Command");

    public static string GetQuerySpecClassName(this ApplicationProject project, string entity)
        => project.CodeNamingConvention.GetClassName($"{entity}QuerySpec");

    public static string GetBaseQuerySpecClassName(this ApplicationProject project)
        => project.CodeNamingConvention.GetClassName("BaseQuerySpec");

    public static string GetQuerySpecExtensionsClassName(this ApplicationProject project)
        => project.CodeNamingConvention.GetClassName("QuerySpecExtensions");

    #endregion

    public static ApplicationProject Scaffold(this ApplicationProject project, Action<ApplicationProjectSettings> action = null)
    {
        ScaffoldApplicationConfigureServices(project);
        ScaffoldDbContextInterface(project);
        ScaffoldFeatures(project);
        ScaffoldQuerySpecs(project);

        return project;
    }

    internal static void ScaffoldApplicationConfigureServices(ApplicationProject project)
    {
        project.Scaffold(project.GetApplicationConfigureServicesClassDef(), project.GetApplicationDirectory());
    }

    internal static void ScaffoldDbContextInterface(ApplicationProject project)
    {
        project.Scaffold(project.GetDbContextInterfaceDef(), project.GetApplicationCommonDirectory());
    }

    internal static void ScaffoldFeatures(ApplicationProject project)
    {
        foreach (var feature in project.Features)
        {
            foreach (var dbObject in feature.DbObjects)
            {
                if (!dbObject.Type.Contains("TABLE"))
                    continue;

                var table = project.Database.FindTable(dbObject.FullName);
                var names = new
                {
                    Feature = $"{dbObject.Name}s",
                    SingleQuery = $"Get{table.Name}",
                    ComposedQuery = $"Get{table.Name}s",
                    CreateCommand = $"Create{dbObject.Name}"
                };

                project.Scaffold(project.GetSingleQueryClassDef(names.Feature, names.SingleQuery, table), project.GetApplicationFeaturesDirectory(names.Feature));
                project.Scaffold(project.GetSingleQueryHandlerClassDef(names.Feature, names.SingleQuery), project.GetApplicationFeaturesDirectory(names.Feature));

                project.Scaffold(project.GetComposedQueryClassDef(names.Feature, names.ComposedQuery, table), project.GetApplicationFeaturesDirectory(names.Feature));
                project.Scaffold(project.GetComposedQueryHandlerClassDef(names.Feature, names.ComposedQuery), project.GetApplicationFeaturesDirectory(names.Feature));

                project.Scaffold(project.GetCreateCommandClassDef(names.Feature, names.CreateCommand, table), project.GetApplicationFeaturesDirectory(names.Feature));
                project.Scaffold(project.GetCreateCommandHandlerClassDef(names.Feature, names.CreateCommand, table), project.GetApplicationFeaturesDirectory(names.Feature));
            }
        }
    }

    internal static void ScaffoldQuerySpecs(ApplicationProject project)
    {
        project.Scaffold(project.GetBaseQuerySpecInterfaceDef(), project.GetApplicationQuerySpecsDirectory());
        project.Scaffold(project.GetBaseQuerySpecClassDef(), project.GetApplicationQuerySpecsDirectory());

        project.Scaffold(project.GetQuerySpecExtensionsClassDef(), project.GetApplicationQuerySpecsDirectory());

        foreach (var feature in project.Features)
        {
            foreach (var dbObject in feature.DbObjects)
            {
                if (!dbObject.Type.Contains("TABLE"))
                    continue;

                var table = project.Database.FindTable(dbObject.FullName);
                var names = new
                {
                    Entity = dbObject.Name,
                    Query = $"Get{dbObject.Name}",
                };

                project.Scaffold(project.GetQuerySpecClassDef(names.Query, names.Entity, table), project.GetApplicationQuerySpecsDirectory());
            }
        }
    }
}
