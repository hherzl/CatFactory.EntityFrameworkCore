using CatFactory.DotNetCore;

namespace CatFactory.EfCore.Definitions
{
    public class EntityInterfaceDefinition : CSharpInterfaceDefinition
    {
        public EntityInterfaceDefinition(EfCoreProject project)
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            Namespace = Project.GetEntityLayerNamespace();
            Name = "IEntity";
        }
    }
}
