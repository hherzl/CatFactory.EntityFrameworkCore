namespace CatFactory.EntityFrameworkCore;

public static class EntityFrameworkCoreProjectInfrastructureExtensions
{
    public static string GetInfrastructureDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.ProjectNamespaces.Infrastructure}");
    public static string GetInfrastructurePersistenceDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.GetInfrastructureDirectory(), project.ProjectNamespaces.Persistence);

    public static string GetInfrastructureConfigurationsDirectory(this EntityFrameworkCoreProject project)
        => Path.Combine(project.GetInfrastructurePersistenceDirectory(), project.ProjectNamespaces.Configurations);

    public static string GetInfrastructurePersistenceNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Infrastructure, project.ProjectNamespaces.Persistence);

    public static string GetInfrastructureNs(this EntityFrameworkCoreProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.Infrastructure);
}
