using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class AuditEntityInterfaceBuilder
    {
        public static AuditEntityInterfaceDefinition GetAuditEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new AuditEntityInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("System");

            interfaceDefinition.Namespace = project.GetEntityLayerNamespace();
            interfaceDefinition.Name = "IAuditEntity";

            interfaceDefinition.Implements.Add("IEntity");

            interfaceDefinition.Properties.Add(new PropertyDefinition("String", "CreationUser"));
            interfaceDefinition.Properties.Add(new PropertyDefinition("DateTime?", "CreationDateTime"));
            interfaceDefinition.Properties.Add(new PropertyDefinition("String", "LastUpdateUser"));
            interfaceDefinition.Properties.Add(new PropertyDefinition("DateTime?", "LastUpdateDateTime"));

            return interfaceDefinition;
        }
    }
}
