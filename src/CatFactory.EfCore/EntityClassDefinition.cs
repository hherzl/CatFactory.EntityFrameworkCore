using System;
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
            Namespaces = new List<String>()
            {
                "System"
            };

            Name = dbObject.GetEntityName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            for (var i = 0; i < dbObject.Columns.Count; i++)
            {
                var column = dbObject.Columns[i];

                Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
            }
        }
    }
}
