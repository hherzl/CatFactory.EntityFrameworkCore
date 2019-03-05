using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore
{
    public class EntityFrameworkCoreProjectSettings : IProjectSettings
    {
        public ValidationResult Validate()
        {
            // todo: Add implementation
            throw new NotImplementedException();
        }

        public EntityFrameworkCoreProjectSettings()
        {
        }

        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; } = true;

        public bool UseAutomaticPropertiesForEntities { get; set; } = true;

        public bool EnableDataBindings { get; set; }

        public bool UseDataAnnotations { get; set; }

        [Obsolete("Temporarily disabled")] public bool UseMefForEntitiesMapping { get; set; } = true;

        public bool DeclareDbSetPropertiesInDbContext { get; } = true;

        public bool DeclareNavigationProperties { get; set; } = true;

        public bool DeclareNavigationPropertiesAsVirtual { get; set; }

        public string NavigationPropertyEnumerableNamespace { get; set; } = "System.Collections.ObjectModel";

        public string NavigationPropertyEnumerableType { get; set; } = "Collection";

        public string ConcurrencyToken { get; set; }

        public string EntityInterfaceName { get; set; } = "IEntity";

        public AuditEntity AuditEntity { get; set; }

        public bool EntitiesWithDataContracts { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_backingFields;

        public List<string> BackingFields
        {
            get { return m_backingFields ?? (m_backingFields = new List<string>()); }
            set { m_backingFields = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_insertExclusions;

        public List<string> InsertExclusions
        {
            get { return m_insertExclusions ?? (m_insertExclusions = new List<string>()); }
            set { m_insertExclusions = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_updateExclusions;

        public List<string> UpdateExclusions
        {
            get { return m_updateExclusions ?? (m_updateExclusions = new List<string>()); }
            set { m_updateExclusions = value; }
        }

        /// <summary>
        /// When true the Database DefaultSchema is also used as a namespace and folder
        /// </summary>
        public Boolean DefaultSchemaAsSubdirectory { get; set; }

        /// <summary>
        /// Navigation Property Enumerable Interface Type
        /// Typically ICollection
        /// </summary>
        public String NavigationPropertyEnumerableInterfaceType { get; set; }

        /// <summary>
        /// DefaultSchemaAsNamespace
        /// default = false;
        /// When true use DefaultSchema (dbo) as namespace and class
        /// </summary>
        public Boolean DefaultSchemaAsNamespace { get; set; }
    }
}
