using System;
using System.IO;

namespace CatFactory.EfCore
{
    public static class ProjectExtensions
    {
        public static String GetEntityLayerDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "EntityLayer");
        }

        public static String GetDataLayerDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "DataLayer");
        }

        public static String GetDataLayerMappingDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "DataLayer", "Mapping");
        }

        public static String GetDataLayerContractsDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "DataLayer", "Contracts");
        }

        public static String GetDataLayerDataContractsDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "DataLayer", "DataContracts");
        }

        public static String GetDataLayerRepositoriesDirectory(this Project project)
        {
            return Path.Combine(project.OutputDirectory, "DataLayer", "Repositories");
        }
    }
}
