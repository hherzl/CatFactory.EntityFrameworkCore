using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;

namespace CatFactory.EntityFrameworkCore
{
    public static class ProjectFeatureExtensions
    {
        public static ICodeNamingConvention NamingConvention;

        static ProjectFeatureExtensions()
        {
            NamingConvention = new DotNetNamingConvention();
        }

        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;

        public static string GetInterfaceRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => NamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => NamingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));
    }
}
