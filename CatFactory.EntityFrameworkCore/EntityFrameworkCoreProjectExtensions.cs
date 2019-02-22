using System;
using System.IO;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        public static string GetParameterName(this EntityFrameworkCoreProject project, Column column)
            => project.CodeNamingConvention.GetParameterName(column.Name);

        public static string GetNavigationPropertyName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}List", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetEntityName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.CodeNamingConvention.GetClassName(dbObject.Name);

        public static string GetPluralName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.NamingService.Pluralize(project.GetEntityName(dbObject));

        public static string GetDbContextName(this EntityFrameworkCoreProject project, Database database)
            => project.CodeNamingConvention.GetClassName(string.Format("{0}DbContext", database.Name));

        public static string GetDbSetPropertyName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.NamingService.Pluralize(project.GetEntityName(dbObject));

        public static string GetEntityConfigurationName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}Configuration", project.GetEntityName(dbObject));

        public static string GetFullEntityName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.CodeNamingConvention.GetClassName(dbObject.Name);

        public static string GetDataContractName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}Dto", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetGetAllRepositoryMethodName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("Get{0}", project.GetPluralName(dbObject));

        public static string GetGetRepositoryMethodName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("Get{0}Async", project.GetEntityName(dbObject));

        public static string GetGetByUniqueRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table, Unique unique)
            => string.Format("Get{0}By{1}Async", project.GetEntityName(table), string.Join("And", unique.Key.Select(item => project.CodeNamingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Add{0}Async", project.GetEntityName(table));

        public static string GetUpdateRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Update{0}Async", project.GetEntityName(table));

        public static string GetRemoveRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Remove{0}Async", project.GetEntityName(table));

        public static string GetScalarFunctionMethodName(this EntityFrameworkCoreProject project, ScalarFunction scalarFunction)
            => string.Format("{0}{1}", project.CodeNamingConvention.GetClassName(scalarFunction.Schema), project.CodeNamingConvention.GetClassName(scalarFunction.Name));

        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.CodeNamingConvention.GetNamespace(project.ProjectNamespaces.EntityLayer));

        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.Name, project.ProjectNamespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", new string[] { project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer });

        public static string GetDataLayerConfigurationsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations);

        public static string GetDataLayerConfigurationsNamespace(this EntityFrameworkCoreProject project, string schema)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations, schema);

        public static string GetDataLayerContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.Name), project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);

        public static string GetEntityLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer);

        public static string GetEntityLayerDirectory(this EntityFrameworkCoreProject project, string schema)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer, schema);

        public static string GetDataLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer);

        public static string GetDataLayerDirectory(this EntityFrameworkCoreProject project, string subdirectory)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, subdirectory);

        public static string GetDataLayerConfigurationsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations);

        public static string GetDataLayerConfigurationsDirectory(this EntityFrameworkCoreProject project, string schema)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Configurations, schema);

        public static string GetDataLayerContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);

        public static PropertyDefinition GetChildNavigationProperty(this EntityFrameworkCoreProject project, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table, ForeignKey foreignKey)
        {
            var propertyType = string.Format("{0}<{1}>", projectSelection.Settings.NavigationPropertyEnumerableType, project.GetEntityName(table));

            return new PropertyDefinition(propertyType, project.GetNavigationPropertyName(table))
            {
                AccessModifier = AccessModifier.Public,
                IsVirtual = projectSelection.Settings.DeclareNavigationPropertiesAsVirtual
            };
        }

        public static EntityFrameworkCoreProject GlobalSelection(this EntityFrameworkCoreProject project, Action<EntityFrameworkCoreProjectSettings> action = null)
        {
            var settings = new EntityFrameworkCoreProjectSettings();

            var selection = project.Selections.FirstOrDefault(item => item.IsGlobal);

            if (selection == null)
            {
                selection = new ProjectSelection<EntityFrameworkCoreProjectSettings>
                {
                    Pattern = ProjectSelection<EntityFrameworkCoreProjectSettings>.GlobalPattern,
                    Settings = settings
                };

                project.Selections.Add(selection);
            }
            else
            {
                settings = selection.Settings;
            }

            action?.Invoke(settings);

            return project;
        }

        public static ProjectSelection<EntityFrameworkCoreProjectSettings> GlobalSelection(this EntityFrameworkCoreProject project)
            => project.Selections.FirstOrDefault(item => item.IsGlobal);

        public static EntityFrameworkCoreProject Selection(this EntityFrameworkCoreProject project, string pattern, Action<EntityFrameworkCoreProjectSettings> action = null)
        {
            var selection = project.Selections.FirstOrDefault(item => item.Pattern == pattern);

            if (selection == null)
            {
                var globalSettings = project.GlobalSelection().Settings;

                selection = new ProjectSelection<EntityFrameworkCoreProjectSettings>
                {
                    Pattern = pattern,
                    Settings = new EntityFrameworkCoreProjectSettings
                    {
                        ForceOverwrite = globalSettings.ForceOverwrite,
                        SimplifyDataTypes = globalSettings.SimplifyDataTypes,
                        UseAutomaticPropertiesForEntities = globalSettings.UseAutomaticPropertiesForEntities,
                        EnableDataBindings = globalSettings.EnableDataBindings,
                        UseDataAnnotations = globalSettings.UseDataAnnotations,
                        DeclareNavigationProperties = globalSettings.DeclareNavigationProperties,
                        DeclareNavigationPropertiesAsVirtual = globalSettings.DeclareNavigationPropertiesAsVirtual,
                        NavigationPropertyEnumerableNamespace = globalSettings.NavigationPropertyEnumerableNamespace,
                        NavigationPropertyEnumerableType = globalSettings.NavigationPropertyEnumerableType,
                        ConcurrencyToken = globalSettings.ConcurrencyToken,
                        EntityInterfaceName = globalSettings.EntityInterfaceName,
                        AuditEntity = globalSettings.AuditEntity == null ? null : new AuditEntity
                        {
                            CreationUserColumnName = globalSettings.AuditEntity.CreationUserColumnName,
                            CreationDateTimeColumnName = globalSettings.AuditEntity.CreationDateTimeColumnName,
                            LastUpdateUserColumnName = globalSettings.AuditEntity.LastUpdateUserColumnName,
                            LastUpdateDateTimeColumnName = globalSettings.AuditEntity.LastUpdateDateTimeColumnName
                        },
                        EntitiesWithDataContracts = globalSettings.EntitiesWithDataContracts,
                        BackingFields = globalSettings.BackingFields.Select(item => item).ToList(),
                        InsertExclusions = globalSettings.InsertExclusions.Select(item => item).ToList(),
                        UpdateExclusions = globalSettings.UpdateExclusions.Select(item => item).ToList()
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }

        [Obsolete("Use Selection method.")]
        public static EntityFrameworkCoreProject Select(this EntityFrameworkCoreProject project, string pattern, Action<EntityFrameworkCoreProjectSettings> action = null)
            => project.Selection(pattern, action);
    }
}
