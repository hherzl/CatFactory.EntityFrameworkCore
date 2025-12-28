namespace CatFactory.EntityFrameworkCore;

public static class EntityFrameworkCoreProjectApplicationExtensions
{
    public static string GetApplicationDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.ProjectNamespaces.Application}");

    public static string GetApplicationCommonDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.GetApplicationDirectory(), project.ProjectNamespaces.Common);

    public static string GetApplicationFeatureQueriesDirectory(this EntityFrameworkCoreProject project, string feature, string query)
        => Path.Combine(project.GetApplicationDirectory(), project.ProjectNamespaces.Features, feature, project.ProjectNamespaces.Queries, query);

    public static string GetApplicationFeatureCommandsDirectory(this EntityFrameworkCoreProject project, string feature, string query)
        => Path.Combine(project.GetApplicationDirectory(), project.ProjectNamespaces.Features, feature, project.ProjectNamespaces.Commands, query);

    public static string GetApplicationQuerySpecsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.GetApplicationDirectory(), project.ProjectNamespaces.QuerySpecs);

    public static string GetApplicationNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Application);

    public static string GetApplicationCommonNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.ProjectNamespaces.Common);

    public static string GetApplicationFeatureQueriesNs(this EntityFrameworkCoreProject project, string feature, string query)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.ProjectNamespaces.Features, feature, project.ProjectNamespaces.Queries, query);

    public static string GetApplicationQuerySpecsNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.GetApplicationNs(), project.ProjectNamespaces.QuerySpecs);
}
