namespace CatFactory.EfCore.Definitions.Extensions
{
    public static class EntityInterfaceBuilder
    {
        public static EntityInterfaceDefinition GetEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new EntityInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Name = "IEntity"
            };
    }
}
