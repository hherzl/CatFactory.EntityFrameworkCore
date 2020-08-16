using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityInterfaceBuilder
    {
        public static EntityInterfaceDefinition GetEntityInterfaceDefinition(this EntityFrameworkCoreProject project, bool isDomainDrivenDesign)
            => new EntityInterfaceDefinition
            {
                Namespace = isDomainDrivenDesign ? project.GetDomainModelsNamespace() : project.GetEntityLayerNamespace(),
                Namespaces =
                {
                    "System"
                },
                AccessModifier = AccessModifier.Public,
                Name = "IEntity"
            };
    }
}
