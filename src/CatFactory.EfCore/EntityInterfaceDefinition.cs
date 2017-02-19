using CatFactory.DotNetCore;

namespace CatFactory.EfCore
{
    public class EntityInterfaceDefinition : CSharpInterfaceDefinition
    {
        public EntityInterfaceDefinition()
        {
            Name = "IEntity";
        }
    }
}
