using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class RepositoryInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetRepositoryInterfaceDefinition(this EfCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("System");
            interfaceDefinition.Namespaces.Add("System.Threading.Tasks");

            interfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();
            interfaceDefinition.Name = "IRepository";

            interfaceDefinition.Implements.Add("IDisposable");

            interfaceDefinition.Methods.Add(new MethodDefinition("Int32", "CommitChanges"));
            interfaceDefinition.Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync"));

            return interfaceDefinition;
        }
    }
}
