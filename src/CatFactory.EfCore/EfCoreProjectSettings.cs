using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatFactory.EfCore
{
    public class EfCoreProjectSettings : ProjectSettings
    {
        public Boolean SimplifyDataTypes { get; set; }

        public Boolean UseAutomaticPropertiesForEntities { get; set; } = true;

        public Boolean EnableDataBindings { get; set; }

        public Boolean UseDataAnnotations { get; set; }

        public Boolean UseMefForEntitiesMapping { get; set; } = true;

        public Boolean DeclareDbSetPropertiesInDbContext { get; set; }

        public Boolean DeclareNavigationPropertiesAsVirtual { get; set; }

        public String NavigationPropertyEnumerableNamespace { get; set; } = "System.Collections.ObjectModel";

        public String NavigationPropertyEnumerableType { get; set; } = "Collection";

        public String ConcurrencyToken { get; set; }

        public String EntityInterfaceName { get; set; } = "IEntity";

        public AuditEntity AuditEntity { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<String> m_backingFields;

        public List<String> EntitiesWithDataContracts
        {
            get
            {
                return m_entitiesWithDataContracts ?? (m_entitiesWithDataContracts = new List<String>());
            }
            set
            {
                m_entitiesWithDataContracts = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<String> m_entitiesWithDataContracts;

        public List<String> BackingFields
        {
            get
            {
                return m_backingFields ?? (m_backingFields = new List<String>());
            }
            set
            {
                m_backingFields = value;
            }
        }

        // todo: add logic to show author's info
        public AuthorInfo AuthorInfo { get; set; }
    }
}
