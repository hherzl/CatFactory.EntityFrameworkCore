using CatFactory.CodeFactory.Scaffolding;

namespace CatFactory.EntityFrameworkCore
{
    public static class ProjectFeatureExtensions
    {
        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;
    }
}
