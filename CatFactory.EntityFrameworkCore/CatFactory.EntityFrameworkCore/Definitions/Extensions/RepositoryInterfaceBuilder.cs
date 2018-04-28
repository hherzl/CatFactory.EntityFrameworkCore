using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions.Extensions
{
    public static class RepositoryInterfaceBuilder
    {
        public static RepositoryInterfaceDefinition GetRepositoryInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new RepositoryInterfaceDefinition();

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
