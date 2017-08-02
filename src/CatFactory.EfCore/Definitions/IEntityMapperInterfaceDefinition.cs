using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class IEntityMapperInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IEntityMapperInterfaceDefinition(EfCoreProject project)
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Namespace = Project.GetDataLayerMappingNamespace();

            Name = "IEntityMapper";

            Properties.Add(new PropertyDefinition("IEnumerable<IEntityMap>", "Mappings") { IsReadOnly = true });

            Methods.Add(new MethodDefinition("void", "MapEntities", new ParameterDefinition("ModelBuilder", "modelBuilder")));
        }
    }
}
