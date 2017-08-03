using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class EntityMapInterfaceDefinition : CSharpInterfaceDefinition
    {
        public EntityMapInterfaceDefinition(EfCoreProject project)
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Namespace = Project.GetDataLayerMappingNamespace();

            Name = "IEntityMap";

            Methods.Add(new MethodDefinition("void", "Map", new ParameterDefinition("ModelBuilder", "modelBuilder")));
        }
    }
}
