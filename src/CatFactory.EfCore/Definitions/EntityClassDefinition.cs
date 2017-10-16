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
    public static class EntityClassDefinition
    {
        public static CSharpClassDefinition GetEntityClassDefinition(this ITable table, EfCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");

            if (project.Settings.UseDataAnnotations)
            {
                classDefinition.Namespaces.Add("System.ComponentModel.DataAnnotations");
                classDefinition.Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            if (project.Settings.EnableDataBindings)
            {
                classDefinition.Namespaces.Add("System.ComponentModel");

                classDefinition.Implements.Add("INotifyPropertyChanged");

                classDefinition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            classDefinition.Namespace = table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

            classDefinition.Name = table.GetSingularName();

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            var columns = table.Columns;

            var typeResolver = new ClrTypeResolver();

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.PrimaryKey.GetColumns(table).First();

                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(typeResolver.Resolve(column.Type), column.GetParameterName()))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("{0} = {1};", column.GetPropertyName(), column.GetParameterName())
                    }
                });
            }

            if (!String.IsNullOrEmpty(table.Description))
            {
                classDefinition.Documentation.Summary = table.Description;
            }

            foreach (var column in columns)
            {
                if (project.Settings.EnableDataBindings)
                {
                    classDefinition.AddViewModelProperty(typeResolver.Resolve(column.Type), column.GetPropertyName());
                }
                else
                {
                    if (project.Settings.BackingFields.Contains(table.GetFullColumnName(column)))
                    {
                        classDefinition.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
                    }
                    else if (project.Settings.UseAutomaticPropertiesForEntities)
                    {
                        classDefinition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
                    }
                    else
                    {
                        classDefinition.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
                    }
                }
            }

            if (project.Settings.AuditEntity == null)
            {
                classDefinition.Implements.Add("IEntity");
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
                    classDefinition.Implements.Add(project.Settings.EntityInterfaceName);
                }
                else
                {
                    classDefinition.Implements.Add("IEntity");
                }
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                var foreignTable = project.Database.FindTableByFullName(foreignKey.References);

                if (foreignTable == null)
                {
                    continue;
                }

                classDefinition.Namespaces.AddUnique(foreignTable.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(foreignTable.Schema));

                classDefinition.Namespace = table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

                classDefinition.Properties.Add(foreignKey.GetParentNavigationProperty(project, foreignTable));
            }

            foreach (var child in project.Database.Tables)
            {
                if (table.FullName == child.FullName)
                {
                    continue;
                }

                foreach (var foreignKey in child.ForeignKeys)
                {
                    if (foreignKey.References.EndsWith(table.FullName))
                    {
                        classDefinition.Namespaces.AddUnique(project.Settings.NavigationPropertyEnumerableNamespace);
                        classDefinition.Namespaces.AddUnique(child.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(child.Schema));

                        var navigationProperty = project.GetChildNavigationProperty(child, foreignKey);

                        if (classDefinition.Properties.FirstOrDefault(item => item.Name == navigationProperty.Name) == null)
                        {
                            classDefinition.Properties.Add(navigationProperty);
                        }
                    }
                }
            }

            if (project.Settings.SimplifyDataTypes)
            {
                classDefinition.SimplifyDataTypes();
            }

            return classDefinition;
        }

        public static CSharpClassDefinition GetEntityClassDefinition(this IView view, EfCoreProject project)
        {
            var typeResolver = new ClrTypeResolver();

            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");

            classDefinition.Namespace = view.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema);

            classDefinition.Name = view.GetSingularName();

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            if (!String.IsNullOrEmpty(view.Description))
            {
                classDefinition.Documentation.Summary = view.Description;
            }

            foreach (var column in view.Columns)
            {
                classDefinition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
            }

            if (project.Settings.SimplifyDataTypes)
            {
                classDefinition.SimplifyDataTypes();
            }

            return classDefinition;
        }
    }
}
