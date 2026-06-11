using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class EntityClassBuilder
{
    public static EntityClassDefinition GetEntityClassDef(this DomainProject project, ITable table)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = project.GetDomainEntitiesNs(),
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

        if (table.PrimaryKey != null)
        {
            var constructor = new ClassConstructorDefinition
            {
                AccessModifier = AccessModifier.Public
            };

            foreach (var key in table.GetColumnsFromConstraint(table.PrimaryKey))
            {
                var col = (Column)key;
                var propType = project.Database.ResolveDatabaseType(col);

                constructor.Parameters.Add(new ParameterDefinition(propType, project.GetParamName(col)));
                constructor.Lines.Add(new CodeLine("{0} = {1};", project.GetPropName(key.Name), project.GetParamName(col)));
            }

            definition.Constructors.Add(constructor);
        }

        var columns = table.Columns;

        foreach (var column in columns)
        {
            var propType = project.Database.ResolveDatabaseType(column);

            if (projectSelection.Settings.BackingFields.Contains(table.GetFullColumnName(column)))
            {
                definition.AddPropWithField(propType, project.GetPropName(table, column));
            }
            else if (projectSelection.Settings.UseAutomaticPropertiesForEntities)
            {
                definition.Properties.Add(new(AccessModifier.Public, propType, project.GetPropName(table, column))
                {
                    IsAutomatic = true
                });
            }
            else
            {
                definition.AddPropWithField(propType, project.GetPropName(table, column));
            }
        }

        if (projectSelection.Settings.AuditEntity == null)
        {
            if (projectSelection.Settings.HasEntityInterfaceName)
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

            if (count == projectSelection.Settings.AuditEntity.Names.Count())
                definition.Implements.Add("IAuditEntity");
            else
                definition.Implements.Add("IEntity");
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        if (!projectSelection.Settings.DeclareNavigationProperties)
            return definition;

        foreach (var foreignKey in table.ForeignKeys)
        {
            var foreignTable = project.Database.FindTable(foreignKey.References);
            if (foreignTable == null)
                continue;

            if (definition.Namespace != project.GetDomainEntitiesNs())
                definition.Namespaces.AddUnique(project.GetDomainEntitiesNs());

            var foreignKeyProp = foreignKey.GetParentNavigationProp(foreignTable, project);
            if (!definition.Properties.Any(item => item.Name == foreignKeyProp.Name))
                definition.Properties.Add(foreignKeyProp);
        }

        foreach (var child in project.Database.Tables)
        {
            foreach (var foreignKey in child.ForeignKeys)
            {
                if (foreignKey.References.EndsWith(table.FullName))
                {
                    definition.Namespaces.AddUnique(projectSelection.Settings.NavigationPropertyEnumerableNamespace);

                    var navigationProp = project.GetChildNavigationProp(projectSelection, child, foreignKey);

                    if (!definition.Properties.Any(item => item.Name == navigationProp.Name))
                        definition.Properties.Add(navigationProp);
                }
            }
        }

        return definition;
    }

    public static EntityClassDefinition GetEntityClassDef(this DomainProject project, IView view)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = project.GetDomainEntitiesNs(),
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
            definition.Properties.Add(new PropertyDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = project.Database.ResolveDatabaseType(column),
                Name = project.GetPropName(view, column),
                IsAutomatic = true
            });
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        return definition;
    }

    public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, ITable table, bool isDomainDrivenDesign)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            AccessModifier = AccessModifier.Public,
            Name = project.GetEntityName(table),
            IsPartial = true,
            Constructors =
            {
                new ClassConstructorDefinition(AccessModifier.Public)
            },
            DbObject = table
        };

        if (isDomainDrivenDesign)
            definition.Namespace = project.Database.HasDefaultSchema(table) ? project.GetDomainModelsNamespace() : project.GetDomainModelsNamespace(table.Schema);
        else
            definition.Namespace = project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

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
            var ctor = new ClassConstructorDefinition
            {
                AccessModifier = AccessModifier.Public
            };

            foreach (var key in table.GetColumnsFromConstraint(table.PrimaryKey))
            {
                var column = (Column)key;

                var propType = project.Database.ResolveDatabaseType(column);

                ctor.Parameters.Add(new ParameterDefinition(propType, project.GetParameterName(column)));

                ctor.Lines.Add(new CodeLine("{0} = {1};", project.GetPropName(key.Name), project.GetParameterName(column)));
            }

            definition.Constructors.Add(ctor);
        }

        var columns = table.Columns;

        foreach (var column in columns)
        {
            var propType = project.Database.ResolveDatabaseType(column);

            if (projectSelection.Settings.EnableDataBindings)
            {
                definition.AddPropWithField(propType, project.GetPropName(table, column));
            }
            else
            {
                if (projectSelection.Settings.BackingFields.Contains(table.GetFullColumnName(column)))
                {
                    definition.AddPropWithField(propType, project.GetPropName(table, column));
                }
                else if (projectSelection.Settings.UseAutomaticPropertiesForEntities)
                {
                    definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, propType, project.GetPropName(table, column))
                    {
                        IsAutomatic = true
                    });
                }
                else
                {
                    definition.AddPropWithField(propType, project.GetPropName(table, column));
                }
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

            if (count == projectSelection.Settings.AuditEntity.Names.Count())
                definition.Implements.Add("IAuditEntity");
            else
                definition.Implements.Add("IEntity");
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        if (!projectSelection.Settings.DeclareNavigationProperties)
            return definition;

        foreach (var foreignKey in table.ForeignKeys)
        {
            var foreignTable = project.Database.FindTable(foreignKey.References);
            if (foreignTable == null)
                continue;

            if (isDomainDrivenDesign)
            {
                if (definition.Namespace != project.GetDomainModelsNamespace(foreignTable.Schema))
                {
                    definition
                        .Namespaces
                        .AddUnique(project.Database.HasDefaultSchema(foreignTable) ? project.GetDomainModelsNamespace() : project.GetDomainModelsNamespace(foreignTable.Schema))
                        ;
                }
            }
            else
            {
                if (definition.Namespace != project.GetEntityLayerNamespace(foreignTable.Schema))
                {
                    definition
                        .Namespaces
                        .AddUnique(project.Database.HasDefaultSchema(foreignTable) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(foreignTable.Schema))
                        ;
                }
            }

            var fkProperty = foreignKey.GetParentNavigationProp(foreignTable, project);

            if (!definition.Properties.Any(item => item.Name == fkProperty.Name))
                definition.Properties.Add(fkProperty);
        }

        foreach (var child in project.Database.Tables)
        {
            foreach (var foreignKey in child.ForeignKeys)
            {
                if (foreignKey.References.EndsWith(table.FullName))
                {
                    definition.Namespaces.AddUnique(projectSelection.Settings.NavigationPropertyEnumerableNamespace);

                    if (isDomainDrivenDesign)
                    {
                        if (definition.Namespace != project.GetDomainModelsNamespace(child.Schema))
                            definition.Namespaces.AddUnique(project.Database.HasDefaultSchema(child) ? project.GetDomainModelsNamespace() : project.GetDomainModelsNamespace(child.Schema));
                    }
                    else
                    {
                        if (definition.Namespace != project.GetEntityLayerNamespace(child.Schema))
                            definition.Namespaces.AddUnique(project.Database.HasDefaultSchema(child) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(child.Schema));
                    }

                    var navigationProp = project.GetChildNavigationProperty(projectSelection, child, foreignKey);

                    if (!definition.Properties.Any(item => item.Name == navigationProp.Name))
                        definition.Properties.Add(navigationProp);
                }
            }
        }

        return definition;
    }

    public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, IView view, string ns)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = ns,
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
            definition.Properties.Add(new PropertyDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = project.Database.ResolveDatabaseType(column),
                Name = project.GetPropName(view, column),
                IsAutomatic = true
            });
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        return definition;
    }

    public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, TableFunction tableFunction)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = project.Database.HasDefaultSchema(tableFunction) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(tableFunction.Schema),
            AccessModifier = AccessModifier.Public,
            Name = project.GetEntityResultName(tableFunction),
            IsPartial = true,
            Constructors =
            {
                new ClassConstructorDefinition
                {
                    AccessModifier = AccessModifier.Public
                }
            },
            DbObject = tableFunction
        };

        if (!string.IsNullOrEmpty(tableFunction.Description))
            definition.Documentation.Summary = tableFunction.Description;

        var projectSelection = project.GetSelection(tableFunction);

        foreach (var column in tableFunction.Columns)
        {
            definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropName(column.Name))
            {
                IsAutomatic = true
            });
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        return definition;
    }

    public static EntityClassDefinition GetEntityClassDefinition(this EntityFrameworkCoreProject project, StoredProcedure storedProcedure)
    {
        var definition = new EntityClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = project.Database.HasDefaultSchema(storedProcedure) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(storedProcedure.Schema),
            AccessModifier = AccessModifier.Public,
            Name = project.GetEntityResultName(storedProcedure),
            IsPartial = true,
            Constructors =
            {
                new ClassConstructorDefinition
                {
                    AccessModifier = AccessModifier.Public
                }
            },
            DbObject = storedProcedure
        };

        if (!string.IsNullOrEmpty(storedProcedure.Description))
            definition.Documentation.Summary = storedProcedure.Description;

        var projectSelection = project.GetSelection(storedProcedure);

        if (storedProcedure.ResultSets.Count == 0)
        {
            // todo: Add logic to stored procedures with no result set
        }
        else
        {
            foreach (var prop in storedProcedure.ResultSets)
            {
                definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(prop.Type), project.GetPropName(prop.Name))
                {
                    IsAutomatic = true
                });
            }
        }

        if (projectSelection.Settings.SimplifyDataTypes)
            definition.SimplifyDataTypes();

        return definition;
    }
}
