using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class EntityMapperInterfaceDefinition : CSharpInterfaceDefinition
    {
        public EntityMapperInterfaceDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public void Init()
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
