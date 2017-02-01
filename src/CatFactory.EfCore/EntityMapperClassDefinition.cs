using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityMapperClassDefinition : CSharpClassDefinition
    {
        public EntityMapperClassDefinition()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = "EntityMapper";

            Implements.Add("IEntityMapper");

            Constructors.Add(new ClassConstructorDefinition());

            Properties.Add(new PropertyDefinition("IEnumerable<IEntityMap>", "Mappings"));

            Methods.Add(new MethodDefinition("void", "MapEntities", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("foreach (var item in Mappings)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "item.Map(modelBuilder);"),
                    new CodeLine("}}")
                }
            });
        }
    }
}
