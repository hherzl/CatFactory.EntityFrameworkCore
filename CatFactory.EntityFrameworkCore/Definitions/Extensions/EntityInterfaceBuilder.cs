using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityInterfaceBuilder
    {
        public static EntityInterfaceDefinition GetEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new EntityInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Namespaces =
                {
                    "System"
                },
                AccessModifier = AccessModifier.Public,
                Name = "IEntity"
            };
    }
}
