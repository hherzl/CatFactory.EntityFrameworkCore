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

            if (project.UseDataAnnotations)
            {
                Namespaces.Add("System.ComponentModel.DataAnnotations");
                Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            Name = dbObject.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            var columns = default(IEnumerable<Column>);

            var tableCast = dbObject as ITable;

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

                if (project.AuditEntity == null)
                {
                    Implements.Add("IEntity");
                }
                else
                {
                    var count = 0;

                    foreach (var column in columns)
                    {
                        if (project.AuditEntity.Names.Contains(column.Name))
                        {
                            count += 1;
                        }
                    }

                    if (count == project.AuditEntity.Names.Length)
                    {
                        Implements.Add(project.EntityInterfaceName);
                    }
                    else
                    {
                        Implements.Add("IEntity");
                    }
                }
            }

            if (tableCast != null)
            {
                foreach (var foreignKey in tableCast.ForeignKeys)
                {
                    var foreignTable = project.Database.Tables.FirstOrDefault(item => item.FullName == foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    Properties.Add(foreignKey.GetParentNavigationProperty(project, foreignTable));
                }

                foreach (var child in project.Database.Tables)
                {
                    foreach (var fk in child.ForeignKeys)
                    {
                        if (fk.References == tableCast.FullName)
                        {
                            if (!Namespaces.Contains(project.NavigationPropertyEnumerableNamespace))
                            {
                                Namespaces.Add(project.NavigationPropertyEnumerableNamespace);
                            }

                            Properties.Add(project.GetChildNavigationProperty(child, fk));
                        }
                    }
                }

            }
        }
    }
}
