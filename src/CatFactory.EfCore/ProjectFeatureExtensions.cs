using System;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;

namespace CatFactory.EfCore
{
    public static class ProjectFeatureExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static ProjectFeatureExtensions()
        {
            namingConvention = new DotNetNamingConvention() as ICodeNamingConvention;
        }

        public static String GetInterfaceRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetClassRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetBusinessInterfaceName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}BusinessObject", projectFeature.Name));

        public static String GetBusinessClassName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}BusinessObject", projectFeature.Name));

        public static EfCoreProject GetProject(this ProjectFeature projectFeature)
            => projectFeature.Project as EfCoreProject;
    }
}
