using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class EntityClassDefinition : CSharpClassDefinition
    {
        public EntityClassDefinition(IDbObject dbObject, EfCoreProject project)
        {
            this.DbObject = dbObject;
            this.Project = project;

            Init();
        }

        public IDbObject DbObject { get; }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            Namespaces.Add("System");

            if (Project.Settings.UseDataAnnotations)
            {
                Namespaces.Add("System.ComponentModel.DataAnnotations");
                Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            if (Project.Settings.EnableDataBindings)
            {
                Namespaces.Add("System.ComponentModel");

                Implements.Add("INotifyPropertyChanged");

                Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            Namespace = DbObject.HasDefaultSchema() ? Project.GetEntityLayerNamespace() : Project.GetEntityLayerNamespace(DbObject.Schema);

            Name = DbObject.GetSingularName();

            Constructors.Add(new ClassConstructorDefinition());
            
            var columns = default(IEnumerable<Column>);

            var tableCast = DbObject as ITable;

            if (tableCast != null)
            {
                columns = tableCast.Columns;

                if (!String.IsNullOrEmpty(tableCast.Description))
                {
                    Documentation.Summary = tableCast.Description;
                }
            }

            var viewCast = DbObject as IView;

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
                    if (Project.Settings.EnableDataBindings)
                    {
                        this.AddViewModelProperty(TypeResolver.Resolve(column.Type), column.GetPropertyName());
                    }
                    else
                    {
                        if (Project.Settings.BackingFields.Contains(tableCast.GetFullColumnName(column)))
                        {
                            this.AddPropertyWithField(TypeResolver.Resolve(column.Type), column.GetPropertyName());
                        }
                        else if (Project.Settings.UseAutomaticPropertiesForEntities)
                        {
                            Properties.Add(new PropertyDefinition(TypeResolver.Resolve(column.Type), column.GetPropertyName()));
                        }
                        else
                        {
                            this.AddPropertyWithField(TypeResolver.Resolve(column.Type), column.GetPropertyName());
                        }
                    }
                }

                if (Project.Settings.AuditEntity == null)
                {
                    Implements.Add("IEntity");
                }
                else
                {
                    var count = 0;

                    foreach (var column in columns)
                    {
                        if (Project.Settings.AuditEntity.Names.Contains(column.Name))
                        {
                            count += 1;
                        }
                    }

                    if (count == Project.Settings.AuditEntity.Names.Length)
                    {
                        Implements.Add(Project.Settings.EntityInterfaceName);
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
                    Properties.Add(new PropertyDefinition(TypeResolver.Resolve(column.Type), column.GetPropertyName()));
                }
            }

            if (tableCast != null)
            {
                foreach (var fk in tableCast.ForeignKeys)
                {
                    var foreignTable = Project.Database.FindTableByFullName(fk.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    Namespaces.AddUnique(Project.GetEntityLayerNamespace(foreignTable.Schema));

                    Properties.Add(fk.GetParentNavigationProperty(Project, foreignTable));
                }

                foreach (var child in Project.Database.Tables)
                {
                    if (tableCast.FullName == child.FullName)
                    {
                        continue;
                    }

                    foreach (var fk in child.ForeignKeys)
                    {
                        if (fk.References.EndsWith(tableCast.FullName))
                        {
                            Namespaces.AddUnique(Project.GetEntityLayerNamespace(child.Schema));
                            Namespaces.AddUnique(Project.Settings.NavigationPropertyEnumerableNamespace);

                            var navigationProperty = Project.GetChildNavigationProperty(child, fk);

                            if (Properties.FirstOrDefault(item => item.Name == navigationProperty.Name) == null)
                            {
                                Properties.Add(navigationProperty);
                            }
                        }
                    }
                }

            }

            if (Project.Settings.SimplifyDataTypes)
            {
                this.SimplifyDataTypes();
            }
        }
    }
}
