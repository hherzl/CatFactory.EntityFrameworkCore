using CatFactory.CodeFactory.Scaffolding;

namespace CatFactory.EntityFrameworkCore
{
    public static class ProjectFeatureExtensions
    {
        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;

        public static string GetInterfaceRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.GetEntityFrameworkCoreProject().CodeNamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.GetEntityFrameworkCoreProject().CodeNamingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));
    }
}
