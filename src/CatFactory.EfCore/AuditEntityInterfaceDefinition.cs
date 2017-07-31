using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class AuditEntityInterfaceDefinition : CSharpInterfaceDefinition
    {
        public AuditEntityInterfaceDefinition(EfCoreProject project)
        {
            if (project.Settings.AuditEntity == null)
            {
                return;
            }

            Namespaces.Add("System");

            Namespace = project.GetEntityLayerNamespace();
            Name = "IAuditEntity";

            Implements.Add("IEntity");

            Properties.Add(new PropertyDefinition("String", "CreationUser"));
            Properties.Add(new PropertyDefinition("DateTime?", "CreationDateTime"));
            Properties.Add(new PropertyDefinition("String", "LastUpdateUser"));
            Properties.Add(new PropertyDefinition("DateTime?", "LastUpdateDateTime"));
        }
    }
}
