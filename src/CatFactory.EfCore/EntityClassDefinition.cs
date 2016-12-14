using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityClassDefinition : CSharpClassDefinition
    {
        public EntityClassDefinition(ITable table)
        {
            Namespaces.Add("System");

            Name = table.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
            }
        }

        public EntityClassDefinition(IView view)
        {
            Namespaces.Add("System");

            Name = view.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
            }
        }
    }
}
