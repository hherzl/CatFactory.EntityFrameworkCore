using CatFactory.DotNetCore;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetEntityInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new CSharpInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Name = "IEntity"
            };
    }
}
