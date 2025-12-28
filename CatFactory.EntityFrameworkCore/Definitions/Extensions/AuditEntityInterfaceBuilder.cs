using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class AuditEntityInterfaceBuilder
{
    public static AuditEntityInterfaceDefinition GetAuditEntityInterfaceDefinition(this DomainProject project)
        => new()
        {
            Namespaces =
            {
                "System"
            },
            Namespace = project.GetDomainEntitiesNs(),
            AccessModifier = AccessModifier.Public,
            Name = "IAuditEntity",
            Implements =
            {
                "IEntity"
            },
            Properties =
            {
                new("string", "CreationUser"),
                new("DateTime?", "CreationDateTime"),
                new("string", "LastUpdateUser"),
                new("DateTime?", "LastUpdateDateTime")
            }
        };
}
