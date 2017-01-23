using System;
using System.IO;

namespace CatFactory.EfCore
{
    public static class ProjectExtensions
    {
        public static String GetEntityLayerDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);
        }

        public static String GetDataLayerDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);
        }

        public static String GetDataLayerMappingDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Mapping);
        }

        public static String GetDataLayerContractsDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);
        }

        public static String GetDataLayerDataContractsDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);
        }

        public static String GetDataLayerRepositoriesDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);
        }

        public static String GetBusinessLayerDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer);
        }

        public static String GetBusinessLayerContractsDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer, project.Namespaces.Contracts);
        }

        public static String GetBusinessLayerResponsesDirectory(this EfCoreProject project)
        {
            return Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer, project.Namespaces.Responses);
        }
    }
}
