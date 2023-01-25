using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore
{
    public class EntityFrameworkCoreProjectSettings : IProjectSettings
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_backingFields;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_insertExclusions;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_updateExclusions;

        public EntityFrameworkCoreProjectSettings()
        {
            SimplifyDataTypes = true;
            UseAutomaticPropertiesForEntities = true;
            UseMefForEntitiesMapping = true;
            DeclareDbSetPropertiesInDbContext = true;
            NavigationPropertyEnumerableNamespace = "System.Collections.ObjectModel";
            NavigationPropertyEnumerableType = "Collection";
            EntityInterfaceName = "IEntity";
            AddConfigurationForUniquesInFluentAPI = true;
            AddConfigurationForDefaultsInFluentAPI = true;
        }

        // todo: Add implementation
        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; }

        public bool UseAutomaticPropertiesForEntities { get; set; }

        public bool EnableDataBindings { get; set; }

        public bool UseDataAnnotations { get; set; }

        [Obsolete("Temporarily disabled")]
        public bool UseMefForEntitiesMapping { get; set; }

        public bool DeclareDbSetPropertiesInDbContext { get; }

        public bool PluralizeDbSetPropertyNames { get; set; }

        public bool DeclareNavigationProperties { get; set; }

        public bool DeclareNavigationPropertiesAsVirtual { get; set; }

        public string NavigationPropertyEnumerableNamespace { get; set; }

        public string NavigationPropertyEnumerableType { get; set; }

        public string ConcurrencyToken { get; set; }

        public bool HasConcurrencyToken
            => !string.IsNullOrEmpty(ConcurrencyToken);

        public string RowVersion { get; set; }

        public bool HasRowVersion
            => !string.IsNullOrEmpty(RowVersion);

        public string EntityInterfaceName { get; set; }

        public AuditEntity AuditEntity { get; set; }

        public bool EntitiesWithDataContracts { get; set; }

        public bool AddConfigurationForUniquesInFluentAPI { get; set; }

        public bool AddConfigurationForChecksInFluentAPI { get; set; }

        public bool AddConfigurationForDefaultsInFluentAPI { get; set; }

        public bool AddConfigurationForForeignKeysInFluentAPI { get; set; }

        public List<string> BackingFields
        {
            get => m_backingFields ??= new List<string>();
            set => m_backingFields = value;
        }

        public List<string> InsertExclusions
        {
            get => m_insertExclusions ??= new List<string>();
            set => m_insertExclusions = value;
        }

        public List<string> UpdateExclusions
        {
            get => m_updateExclusions ??= new List<string>();
            set => m_updateExclusions = value;
        }

        public bool UseApplyConfigurationsFromAssemblyMethod { get; set; }

        /// <summary>
        /// When true the Database DefaultSchema is also used as a namespace and folder
        /// </summary>
        public bool DefaultSchemaAsSubdirectory { get; set; }

        /// <summary>
        /// Navigation Property Enumerable Interface Type
        /// Typically ICollection
        /// </summary>
        public string NavigationPropertyEnumerableInterfaceType { get; set; }

        /// <summary>
        /// DefaultSchemaAsNamespace
        /// default = false;
        /// When true use DefaultSchema (dbo) as namespace and class
        /// </summary>
        public bool DefaultSchemaAsNamespace { get; set; }
    }
}
