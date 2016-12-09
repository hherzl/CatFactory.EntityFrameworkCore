using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class IEntityMapperInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IEntityMapperInterfaceDefinition()
        {
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = "IEntityMapper";

            Properties.Add(new PropertyDefinition("IEnumerable<IEntityMap>", "Mappings") { IsReadOnly = true });

            Methods.Add(new MethodDefinition("void", "MapEntities")
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("ModelBuilder", "modelBuilder")
                }
            });
        }
    }
}
