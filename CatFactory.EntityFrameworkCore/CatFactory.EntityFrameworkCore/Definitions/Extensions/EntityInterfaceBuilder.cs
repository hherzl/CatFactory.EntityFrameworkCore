namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityInterfaceBuilder
    {
        public static EntityInterfaceDefinition GetEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new EntityInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Namespaces = new System.Collections.Generic.List<string>() { "System" },
                Name = "IEntity"
            };
    }
}
