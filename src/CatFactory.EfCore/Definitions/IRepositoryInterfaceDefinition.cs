using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class IRepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IRepositoryInterfaceDefinition(EfCoreProject project)
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public override void Init()
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
