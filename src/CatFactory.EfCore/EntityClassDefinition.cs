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

            if (project.Settings.UseDataAnnotations)
            {
                Namespaces.Add("System.ComponentModel.DataAnnotations");
                Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            if (project.Settings.EnableDataBindings)
            {
                Namespaces.Add("System.ComponentModel");

                Implements.Add("INotifyPropertyChanged");

                Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
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

            if (tableCast != null)
            {
                foreach (var column in columns)
                {
                    if (project.Settings.EnableDataBindings)
                    {
                        this.AddViewModelProperty(resolver.Resolve(column.Type), column.GetPropertyName());
                    }
                    else
                    {
                        if (project.Settings.UseAutomaticPropertiesForEntities)
                        {
                            Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                        }
                        else
                        {
                            this.AddPropertyWithField(resolver.Resolve(column.Type), column.GetPropertyName());
                        }
                    }
                }

                if (project.Settings.AuditEntity == null)
                {
                    Implements.Add("IEntity");
                }
                else
                {
                    var count = 0;

                    foreach (var column in columns)
                    {
                        if (project.Settings.AuditEntity.Names.Contains(column.Name))
                        {
                            count += 1;
                        }
                    }

                    if (count == project.Settings.AuditEntity.Names.Length)
                    {
                        Implements.Add(project.Settings.EntityInterfaceName);
                    }
                    else
                    {
                        Implements.Add("IEntity");
                    }
                }
            }

            if (viewCast != null)
            {
                foreach (var column in columns)
                {
                    Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), column.GetPropertyName()));
                }
            }

            if (tableCast != null)
            {
                foreach (var fk in tableCast.ForeignKeys)
                {
                    var foreignTable = project.FindTable(fk.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    Properties.Add(fk.GetParentNavigationProperty(project, foreignTable));
                }

                foreach (var child in project.Database.Tables)
                {
                    if (tableCast.FullName == child.FullName)
                    {
                        continue;
                    }

                    foreach (var fk in child.ForeignKeys)
                    {
                        if (fk.References.EndsWith(tableCast.FullName))
                        {
                            if (!Namespaces.Contains(project.Settings.NavigationPropertyEnumerableNamespace))
                            {
                                Namespaces.Add(project.Settings.NavigationPropertyEnumerableNamespace);
                            }

                            var navigationProperty = project.GetChildNavigationProperty(child, fk);

                            if (Properties.FirstOrDefault(item => item.Name == navigationProperty.Name) == null)
                            {
                                Properties.Add(navigationProperty);
                            }
                        }
                    }
                }

            }

            if (project.Settings.SimplifyDataTypes)
            {
                this.SimplifyDataTypes();
            }
        }
    }
}
