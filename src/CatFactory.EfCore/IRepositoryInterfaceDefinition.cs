using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class IRepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IRepositoryInterfaceDefinition(EfCoreProject project)
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Threading.Tasks");

            Namespace = project.GetDataLayerContractsNamespace();
            Name = "IRepository";

            Implements.Add("IDisposable");

            Methods.Add(new MethodDefinition("Int32", "CommitChanges"));
            Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync"));
        }
    }
}
