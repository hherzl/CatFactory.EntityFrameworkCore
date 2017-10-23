using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityMapperClassDefinition
    {
        public static CSharpClassDefinition GetEntityMapperClassDefinition(this EfCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Collections.Generic");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            classDefinition.Namespace = project.GetDataLayerMappingNamespace();

            classDefinition.Name = "EntityMapper";

            classDefinition.Implements.Add("IEntityMapper");

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            classDefinition.Properties.Add(new PropertyDefinition("IEnumerable<IEntityMap>", "Mappings"));

            classDefinition.Methods.Add(new MethodDefinition("void", "MapEntities", new ParameterDefinition("ModelBuilder", "modelBuilder"))
            {
                Lines = new List<ILine>()
                {
                    new CodeLine("foreach (var item in Mappings)"),
                    new CodeLine("{"),
                    new CodeLine(1, "item.Map(modelBuilder);"),
                    new CodeLine("}")
                }
            });

            return classDefinition;
        }
    }
}
