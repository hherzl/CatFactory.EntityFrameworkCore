using System;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityFrameworkCoreProjectSelectionExtensions
    {
        public static ProjectSelection<EntityFrameworkCoreProjectSettings> GetSelection(this EntityFrameworkCoreProject project, IDbObject dbObj)
        {
            // Sales.OrderHeader
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == dbObj.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", dbObj.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.OrderHeader
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", dbObj.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
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
                        UpdateExclusions = globalSettings.UpdateExclusions.Select(item => item).ToList(),
                        AddConfigurationForForeignKeysInFluentAPI = globalSettings.AddConfigurationForForeignKeysInFluentAPI,
                        AddConfigurationForUniquesInFluentAPI = globalSettings.AddConfigurationForUniquesInFluentAPI,
                        AddConfigurationForChecksInFluentAPI = globalSettings.AddConfigurationForChecksInFluentAPI,
                        AddConfigurationForDefaultsInFluentAPI = globalSettings.AddConfigurationForDefaultsInFluentAPI,
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
