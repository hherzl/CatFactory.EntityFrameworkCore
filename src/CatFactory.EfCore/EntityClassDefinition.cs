using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityClassDefinition : CSharpClassDefinition
    {
        public EntityClassDefinition(IDbObject dbObject)
        {
            Namespaces.Add("System");

            Name = dbObject.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            var columns = default(IEnumerable<Column>);

            var tableCast = dbObject as ITable;

            if (tableCast != null)
            {
                columns = tableCast.Columns;
            }

            var viewCast = dbObject as IView;

            if (viewCast != null)
            {
                columns = viewCast.Columns;
            }

            if (tableCast != null || viewCast != null)
            {
                foreach (var column in columns)
                {
                    Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                }
            }
        }
    }
}
