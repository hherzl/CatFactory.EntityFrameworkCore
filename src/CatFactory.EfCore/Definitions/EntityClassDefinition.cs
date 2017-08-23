using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class EntityClassDefinition : CSharpClassDefinition
    {
        public EntityClassDefinition(IDbObject dbObject, EfCoreProject project)
            : base()
        {
            DbObject = dbObject;
            Project = project;

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

            var typeResolver = new ClrTypeResolver();

            if (tableCast != null)
            {
                if (tableCast.PrimaryKey != null && tableCast.PrimaryKey.Key.Count == 1)
                {
                    var column = tableCast.PrimaryKey.GetColumns(tableCast).First();

                    Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(typeResolver.Resolve(column.Type), column.GetParameterName()))
                    {
                        Lines = new List<ILine>()
                        {
                            new CodeLine("{0} = {1};", column.GetPropertyName(), column.GetParameterName())
                        }
                    });
                }

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
                        this.AddViewModelProperty(typeResolver.Resolve(column.Type), column.GetPropertyName());
                    }
                    else
                    {
                        if (Project.Settings.BackingFields.Contains(tableCast.GetFullColumnName(column)))
                        {
                            this.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
                        }
                        else if (Project.Settings.UseAutomaticPropertiesForEntities)
                        {
                            Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
                        }
                        else
                        {
                            this.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
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
                    Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
                }
            }

            if (tableCast != null)
            {
                foreach (var foreignKey in tableCast.ForeignKeys)
                {
                    var foreignTable = Project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }
                    
                    Namespaces.AddUnique(foreignTable.HasDefaultSchema() ? Project.GetEntityLayerNamespace() : Project.GetEntityLayerNamespace(foreignTable.Schema));

                    Namespace = DbObject.HasDefaultSchema() ? Project.GetEntityLayerNamespace() : Project.GetEntityLayerNamespace(DbObject.Schema);

                    Properties.Add(foreignKey.GetParentNavigationProperty(Project, foreignTable));
                }

                foreach (var child in Project.Database.Tables)
                {
                    if (tableCast.FullName == child.FullName)
                    {
                        continue;
                    }

                    foreach (var foreignKey in child.ForeignKeys)
                    {
                        if (foreignKey.References.EndsWith(tableCast.FullName))
                        {
                            Namespaces.AddUnique(Project.Settings.NavigationPropertyEnumerableNamespace);
                            Namespaces.AddUnique(child.HasDefaultSchema() ? Project.GetEntityLayerNamespace() : Project.GetEntityLayerNamespace(child.Schema));

                            var navigationProperty = Project.GetChildNavigationProperty(child, foreignKey);

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
