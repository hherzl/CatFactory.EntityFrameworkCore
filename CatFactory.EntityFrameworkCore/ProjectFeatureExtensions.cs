using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;

namespace CatFactory.EntityFrameworkCore
{
    public static class ProjectFeatureExtensions
    {
        public static ICodeNamingConvention namingConvention;

        static ProjectFeatureExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;

        public static string GetInterfaceRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => namingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => namingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));
    }
}
