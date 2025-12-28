namespace CatFactory.EntityFrameworkCore;

public static partial class EntityFrameworkCoreProjectDomainExtensions
{
    public static string GetDomainDirectory2(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.ProjectNamespaces.Domain}");

    //public static string GetDomainEntitiesDirectory(this EntityFrameworkCoreProject project)
    //    => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.ProjectNamespaces.Domain}", project.ProjectNamespaces.Entities);

    public static string GetDomainExceptionsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.ProjectNamespaces.Domain}", project.ProjectNamespaces.Exceptions);

    public static string GetDomainExceptionNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Domain, project.ProjectNamespaces.Exceptions);

    public static string GetDomainDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory);

    public static string GetDomainModelsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.Models);

    public static string GetDomainQueryModelsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.QueryModels);

    public static string GetDomainConfigurationsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.Configurations);

    public static string GetDomainConfigurationsDirectory(this EntityFrameworkCoreProject project, string schema)
        => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.Configurations, schema);

    public static string GetDomainModelsNamespace(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Domain, project.ProjectNamespaces.Entities);

    public static string GetDomainModelsNamespace(this EntityFrameworkCoreProject project, string ns)
        => string.IsNullOrEmpty(ns) ? GetDomainModelsNamespace(project) : project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Domain, project.ProjectNamespaces.Entities, ns);

    public static string GetDomainQueryModelsNamespace(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.QueryModels);

    public static string GetDomainConfigurationsNamespace(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Infrastructure, project.ProjectNamespaces.Persistence, project.ProjectNamespaces.Configurations);

    public static string GetDomainConfigurationsNamespace(this EntityFrameworkCoreProject project, string ns)
        => string.IsNullOrEmpty(ns) ? GetDomainConfigurationsNamespace(project) : project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Configurations, ns);
}
