using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class AuditEntityInterfaceDefinition : CSharpInterfaceDefinition
    {
        public AuditEntityInterfaceDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public void Init()
        {
            if (Project.Settings.AuditEntity == null)
            {
                return;
            }

            Namespaces.Add("System");

            Namespace = Project.GetEntityLayerNamespace();
            Name = "IAuditEntity";

            Implements.Add("IEntity");

            Properties.Add(new PropertyDefinition("String", "CreationUser"));
            Properties.Add(new PropertyDefinition("DateTime?", "CreationDateTime"));
            Properties.Add(new PropertyDefinition("String", "LastUpdateUser"));
            Properties.Add(new PropertyDefinition("DateTime?", "LastUpdateDateTime"));
        }
    }
}
