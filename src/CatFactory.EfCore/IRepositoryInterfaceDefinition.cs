using CatFactory.DotNetCore;

namespace CatFactory.EfCore
{
    public class IRepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IRepositoryInterfaceDefinition()
        {
            Namespaces.Add("System");

            Name = "IRepository";

            Implements.Add("IDisposable");
        }
    }
}
