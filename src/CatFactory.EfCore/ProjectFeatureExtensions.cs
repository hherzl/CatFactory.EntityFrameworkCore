namespace CatFactory.EfCore
{
    public static class ProjectFeatureExtensions
    {
        public static EntityFrameworkCoreProject GetEntityFrameworkCoreProject(this ProjectFeature projectFeature)
            => projectFeature.Project as EntityFrameworkCoreProject;
    }
}
