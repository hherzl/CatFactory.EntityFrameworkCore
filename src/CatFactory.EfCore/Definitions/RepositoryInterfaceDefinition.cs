using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class RepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public RepositoryInterfaceDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Threading.Tasks");

            Namespace = Project.GetDataLayerContractsNamespace();
            Name = "IRepository";

            Implements.Add("IDisposable");

            Methods.Add(new MethodDefinition("Int32", "CommitChanges"));
            Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync"));
        }
    }
}
