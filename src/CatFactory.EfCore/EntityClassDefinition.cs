using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityClassDefinition : CSharpClassDefinition
    {
        public EntityClassDefinition(IDbObject dbObject, EfCoreProject project)
        {
            Namespaces.Add("System");

            Name = dbObject.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            var columns = default(IEnumerable<Column>);

            var tableCast = dbObject as Table;

            if (tableCast != null)
            {
                columns = tableCast.Columns;

                if (!String.IsNullOrEmpty(tableCast.Description))
                {
                    Documentation.Summary = tableCast.Description;
                }
            }

            var viewCast = dbObject as IView;

            if (viewCast != null)
            {
                columns = viewCast.Columns;

                if (!String.IsNullOrEmpty(viewCast.Description))
                {
                    Documentation.Summary = viewCast.Description;
                }
            }

            if (tableCast != null || viewCast != null)
            {
                foreach (var column in columns)
                {
                    Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                }
            }

            if (tableCast != null)
            {
                foreach (var foreignKey in tableCast.ForeignKeys)
                {
                    var table = project.Database.Tables.FirstOrDefault(item => item.FullName == foreignKey.References);

                    if (table == null)
                    {
                        continue;
                    }

                    Properties.Add(foreignKey.GetParentNavigationProperty(project, table));
                }

                foreach (var child in tableCast.Childs)
                {
                    if (!Namespaces.Contains(project.NavigationPropertyEnumerableNamespace))
                    {
                        Namespaces.Add(project.NavigationPropertyEnumerableNamespace);
                    }

                    var table = project.Database.Tables.FirstOrDefault(item => item.FullName == child);

                    if (table == null)
                    {
                        continue;
                    }

                    Properties.Add(project.GetChildNavigationProperty(table));
                }
            }
        }
    }
}
