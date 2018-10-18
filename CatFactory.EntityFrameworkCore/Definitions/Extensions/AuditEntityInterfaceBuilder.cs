using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class AuditEntityInterfaceBuilder
    {
        public static AuditEntityInterfaceDefinition GetAuditEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new AuditEntityInterfaceDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetEntityLayerNamespace(),
                Name = "IAuditEntity",
                Implements =
                {
                    "IEntity"
                },
                Properties =
                {
                    new PropertyDefinition("string", "CreationUser"),
                    new PropertyDefinition("DateTime?", "CreationDateTime"),
                    new PropertyDefinition("string", "LastUpdateUser"),
                    new PropertyDefinition("DateTime?", "LastUpdateDateTime")
                }
            };
    }
}
