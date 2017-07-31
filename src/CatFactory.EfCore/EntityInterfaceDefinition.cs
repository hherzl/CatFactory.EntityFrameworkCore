using CatFactory.DotNetCore;

namespace CatFactory.EfCore
{
    public class EntityInterfaceDefinition : CSharpInterfaceDefinition
    {
        public EntityInterfaceDefinition(EfCoreProject project)
        {
            Namespace = project.GetEntityLayerNamespace();
            Name = "IEntity";
        }
    }
}
