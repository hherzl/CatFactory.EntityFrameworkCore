using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class EntityInterfaceBuilder
{
    public static EntityInterfaceDefinition GetEntityInterfaceDef(this DomainProject project)
        => new()
        {
            Namespace = project.GetDomainEntitiesNs(),
            Namespaces =
            {
                "System"
            },
            AccessModifier = AccessModifier.Public,
            Name = "IEntity"
        };
}
