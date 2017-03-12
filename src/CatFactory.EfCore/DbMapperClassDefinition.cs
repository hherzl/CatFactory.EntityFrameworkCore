using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class DbMapperClassDefinition : CSharpClassDefinition
    {
        public DbMapperClassDefinition(Database db)
        {
            Namespaces.Add("System.Collections.Generic");

            Name = db.GetDbEntityMapperName();

            BaseClass = "EntityMapper";

            var lines = new List<CodeLine>();

            lines.Add(new CodeLine("Mappings = new List<IEntityMap>()"));

            lines.Add(new CodeLine("{{"));

            for (var i = 0; i < db.Tables.Count; i++)
            {
                var item = db.Tables[i];

                lines.Add(new CodeLine(1, "new {0}(){1}", item.GetMapName(), i == db.Tables.Count - 1 ? String.Empty : ","));
            }

            lines.Add(new CodeLine("}};"));

            Constructors.Add(new ClassConstructorDefinition()
            {
                Lines = lines
            });
        }
    }
}
