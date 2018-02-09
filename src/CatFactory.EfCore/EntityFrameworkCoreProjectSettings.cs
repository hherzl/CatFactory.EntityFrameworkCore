using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory;

namespace CatFactory.EfCore
{
    public class EntityFrameworkCoreProjectSettings : ProjectSettings
    {
        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; }

        public bool UseAutomaticPropertiesForEntities { get; set; } = true;

        public bool EnableDataBindings { get; set; }

        public bool UseDataAnnotations { get; set; }

        public bool UseMefForEntitiesMapping { get; set; } = true;

        public bool DeclareDbSetPropertiesInDbContext { get; set; }

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
            get
            {
                return m_backingFields ?? (m_backingFields = new List<string>());
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
