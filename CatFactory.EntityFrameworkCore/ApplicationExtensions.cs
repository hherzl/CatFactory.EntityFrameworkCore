using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class ApplicationExtensions
{
    public static EntityFrameworkCoreProject ScaffoldApplication(this EntityFrameworkCoreProject project, Action<ApplicationSettings> action = null)
    {
        ScaffoldApplicationConfigureServices(project);
        ScaffoldDbContextInterface(project);
        ScaffoldModels(project);
        ScaffoldFeatures(project);
        ScaffoldQuerySpecs(project);

        return project;
    }

    internal static void ScaffoldApplicationConfigureServices(EntityFrameworkCoreProject project)
    {
        //project.Scaffold(project.GetApplicationConfigureServicesClassDefinition(), project.GetApplicationDirectory());
    }

    internal static void ScaffoldDbContextInterface(EntityFrameworkCoreProject project)
    {
        //project.Scaffold(project.GetDbContextInterfaceDefinition(), project.GetApplicationCommonDirectory());
    }

    internal static void ScaffoldModels(EntityFrameworkCoreProject project)
    {
        foreach (var feature in project.Features)
        {
            foreach (var dbObject in feature.DbObjects)
            {
                if (dbObject.Type != "Table")
                    continue;

                var table = project.Database.FindTable(dbObject.FullName);
                var names = new
                {
                    Feature = table.Name,
                    SingleQuery = $"Get{table.Name}",
                    ComposedQuery = $"Get{table.Name}s",
                    CreateCommand = $"Create{table.Name}"
                };

                //project.Scaffold(project.GetSingleQueryClassDef(names.SingleQuery, table), project.GetApplicationFeatureQueriesDirectory(names.Feature, names.SingleQuery));
                //project.Scaffold(project.GetComposedQueryClassDef(names.ComposedQuery, table), project.GetApplicationFeatureQueriesDirectory(names.Feature, names.ComposedQuery));

                //project.Scaffold(project.GetCreateCommandClassDef(names.CreateCommand, table), project.GetApplicationFeatureCommandsDirectory(names.Feature, names.CreateCommand));
            }
        }
    }

    internal static void ScaffoldFeatures(EntityFrameworkCoreProject project)
    {
        foreach (var feature in project.Features)
        {
            foreach (var dbObject in feature.DbObjects)
            {
                if (dbObject.Type != "Table")
                    continue;

                var table = project.Database.FindTable(dbObject.FullName);
                var names = new
                {
                    Feature = dbObject.Name,
                    SingleQuery = $"Get{table.Name}",
                    ComposedQuery = $"Get{table.Name}s",
                    CreateCommand = $"Create{dbObject.Name}"
                };

                //project.Scaffold(project.GetSingleQueryHandlerClassDef(names.Feature, names.SingleQuery), project.GetApplicationFeatureQueriesDirectory(names.Feature, names.SingleQuery));
                //project.Scaffold(project.GetComposedQueryHandlerClassDef(names.Feature, names.ComposedQuery), project.GetApplicationFeatureQueriesDirectory(names.Feature, names.ComposedQuery));

                //project.Scaffold(project.GetCreateCommandHandlerClassDef(names.Feature, names.CreateCommand), project.GetApplicationFeatureCommandsDirectory(names.Feature, names.CreateCommand));
            }
        }
    }

    internal static void ScaffoldQuerySpecs(EntityFrameworkCoreProject project)
    {
        //project.Scaffold(project.GetBaseQuerySpecClassDefinition(), project.GetApplicationQuerySpecsDirectory());

        foreach (var feature in project.Features)
        {
            foreach (var dbObject in feature.DbObjects)
            {
                if (dbObject.Type != "Table")
                    continue;

                var names = new
                {
                    Feature = dbObject.Name,
                    Query = $"Get{dbObject.Name}",
                };

                //project.Scaffold(project.GetQuerySpecClassDefinition(names.Query), project.GetApplicationQuerySpecsDirectory());
            }
        }
    }
}
