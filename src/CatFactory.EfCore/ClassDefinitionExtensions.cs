using System;
using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class ClassDefinitionExtensions
    {
        public static void AddTableAttribute(this IDotNetClassDefinition classDefinition, ITable table)
        {
            classDefinition.Attributes.Add(new MetadataAttribute("Table", String.Format("\"{0}\"", table.Name))
            {
                Sets = new List<MetadataAttributeSet>()
                {
                    new MetadataAttributeSet("Schema", String.Format("\"{0}\"", table.Schema))
                }
            });
        }
    }
}
