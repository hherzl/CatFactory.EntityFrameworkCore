using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityClassBuilder
    {
        public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, ITable table)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityName(table),
                IsPartial = true,
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public)
                },
                DbObject = table
            };

            if (!string.IsNullOrEmpty(table.Description))
                definition.Documentation.Summary = table.Description;

            var projectSelection = project.GetSelection(table);

            if (projectSelection.Settings.UseDataAnnotations)
            {
                definition.Namespaces.Add("System.ComponentModel.DataAnnotations");
                definition.Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            if (projectSelection.Settings.EnableDataBindings)
            {
                definition.Namespaces.Add("System.ComponentModel");

                definition.Implements.Add("INotifyPropertyChanged");

                definition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            if (table.PrimaryKey != null)
            {
                var constructor = new ClassConstructorDefinition
                {
                    AccessModifier = AccessModifier.Public
                };

                foreach (var key in table.GetColumnsFromConstraint(table.PrimaryKey))
                {
                    var propertyType = string.Empty;

                    if (project.Database.HasTypeMappedToClr(key))
                    {
                        var clrType = project.Database.GetClrMapForType(key);

                        propertyType = clrType.AllowClrNullable ? string.Format("{0}?", clrType.GetClrType().Name) : clrType.GetClrType().Name;
                    }
                    else
                    {
                        propertyType = "object";
                    }

                    constructor.Parameters.Add(new ParameterDefinition(propertyType, project.GetParameterName(key)));

                    constructor.Lines.Add(new CodeLine("{0} = {1};", key.GetPropertyName(), project.GetParameterName(key)));
                }

                definition.Constructors.Add(constructor);
            }

            var columns = table.Columns;

            foreach (var column in columns)
            {
                var propertyType = string.Empty;

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var clrType = project.Database.GetClrMapForType(column);

                    propertyType = clrType.AllowClrNullable ? string.Format("{0}?", clrType.GetClrType().Name) : clrType.GetClrType().Name;
                }
                else
                {
                    propertyType = "object";
                }

                if (projectSelection.Settings.EnableDataBindings)
                {
                    definition.AddViewModelProperty(propertyType, column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : table.GetPropertyNameHack(column));
                }
                else
                {
                    if (projectSelection.Settings.BackingFields.Contains(table.GetFullColumnName(column)))
                        definition.AddPropertyWithField(propertyType, column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : table.GetPropertyNameHack(column));
                    else if (projectSelection.Settings.UseAutomaticPropertiesForEntities)
                        definition.Properties.Add(new PropertyDefinition(propertyType, column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : table.GetPropertyNameHack(column))
                        {
                            AccessModifier = AccessModifier.Public,
                            IsAutomatic = true
                        });
                    else
                        definition.AddPropertyWithField(propertyType, column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : table.GetPropertyNameHack(column));
                }
            }

            if (projectSelection.Settings.AuditEntity == null)
            {
                definition.Implements.Add(projectSelection.Settings.EntityInterfaceName);
            }
            else
            {
                var count = 0;

                foreach (var column in columns)
                {
                    if (projectSelection.Settings.AuditEntity.Names.Contains(column.Name))
                        count += 1;
                }

                if (count == projectSelection.Settings.AuditEntity.Names.Length)
                    definition.Implements.Add("IAuditEntity");
                else
                    definition.Implements.Add("IEntity");
            }

            if (projectSelection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            if (projectSelection.Settings.DeclareNavigationProperties)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    definition.Namespaces
                        .AddUnique(project.Database.HasDefaultSchema(foreignTable) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(foreignTable.Schema));

                    definition.Namespace = project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

                    var fkProperty = foreignKey.GetParentNavigationProperty(foreignTable, project);

                    if (definition.Properties.FirstOrDefault(item => item.Name == fkProperty.Name) == null)
                        definition.Properties.Add(fkProperty);
                }

                foreach (var child in project.Database.Tables)
                {
                    foreach (var foreignKey in child.ForeignKeys)
                    {
                        if (foreignKey.References.EndsWith(table.FullName))
                        {
                            definition.Namespaces
                                .AddUnique(projectSelection.Settings.NavigationPropertyEnumerableNamespace);

                            definition.Namespaces
                                .AddUnique(project.Database.HasDefaultSchema(child) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(child.Schema));

                            var navigationProperty = project.GetChildNavigationProperty(projectSelection, child, foreignKey);

                            if (definition.Properties.FirstOrDefault(item => item.Name == navigationProperty.Name) == null)
                                definition.Properties.Add(navigationProperty);
                        }
                    }
                }
            }

            return definition;
        }

        public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, IView view)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(view) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityName(view),
                IsPartial = true,
                Constructors =
                {
                    new ClassConstructorDefinition
                    {
                        AccessModifier = AccessModifier.Public
                    }
                },
                DbObject = view
            };

            if (!string.IsNullOrEmpty(view.Description))
                definition.Documentation.Summary = view.Description;

            var projectSelection = project.GetSelection(view);

            if (projectSelection.Settings.UseDataAnnotations)
            {
                definition.Namespaces.Add("System.ComponentModel.DataAnnotations");
                definition.Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
            }

            foreach (var column in view.Columns)
            {
                var propertyType = string.Empty;

                if (project.Database.HasTypeMappedToClr(column))
                {
                    var clrType = project.Database.GetClrMapForType(column);

                    propertyType = clrType.AllowClrNullable ? string.Format("{0}?", clrType.GetClrType().Name) : clrType.GetClrType().Name;
                }
                else
                {
                    propertyType = "object";
                }

                definition.Properties.Add(new PropertyDefinition(propertyType, column.HasSameNameEnclosingType(view) ? column.GetNameForEnclosing() : view.GetPropertyNameHack(column))
                {
                    AccessModifier = AccessModifier.Public
                });
            }

            if (projectSelection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }
    }
}
