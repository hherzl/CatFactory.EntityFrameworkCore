using CatFactory.CodeFactory.Scaffolding;

namespace CatFactory.EntityFrameworkCore
{
    public static class ProjectFeatureExtensions
    {
        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;

        public static string GetRepositoryInterfaceName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => string.Format("{0}Repository",
                    projectFeature.GetEntityFrameworkCoreProject().CodeNamingConvention.GetInterfaceName(projectFeature.Name)
            );

        public static string GetRepositoryClassName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => string.Format("{0}Repository",
                projectFeature.GetEntityFrameworkCoreProject().CodeNamingConvention.GetClassName(projectFeature.Name)
            );

        public static string GetQueryExtensionsClassName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => string.Format("{0}{1}QueryExtensions",
                projectFeature.GetEntityFrameworkCoreProject().GetDbContextName(projectFeature.Project.Database),
                projectFeature.Project.CodeNamingConvention.GetClassName(projectFeature.Name)
            );
    }
}
