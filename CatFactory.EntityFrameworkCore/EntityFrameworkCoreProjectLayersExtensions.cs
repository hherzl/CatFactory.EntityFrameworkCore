using System.IO;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityFrameworkCoreProjectLayersExtensions
    {
        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.CodeNamingConvention.GetNamespace(project.ProjectNamespaces.EntityLayer));

        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.Name, project.ProjectNamespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", new string[] { project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer });

        public static string GetDataLayerConfigurationsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations);

        public static string GetDataLayerConfigurationsNamespace(this EntityFrameworkCoreProject project, string schema)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations, schema);

        public static string GetDataLayerContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);

        public static string GetEntityLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer);

        public static string GetEntityLayerDirectory(this EntityFrameworkCoreProject project, string schema)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer, schema);

        public static string GetDataLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer);

        public static string GetDataLayerDirectory(this EntityFrameworkCoreProject project, string subdirectory)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, subdirectory);

        public static string GetDataLayerConfigurationsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations);

        public static string GetDataLayerConfigurationsDirectory(this EntityFrameworkCoreProject project, string schema)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations, schema);

        public static string GetDataLayerContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);
    }
}
