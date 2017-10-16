using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class AuditEntityInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetAuditEntityInterfaceDefinition(this EfCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

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
