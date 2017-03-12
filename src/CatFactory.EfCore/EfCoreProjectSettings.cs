using System;
using System.Collections.Generic;

namespace CatFactory.EfCore
{
    public class EfCoreProjectSettings
    {
        public EfCoreProjectSettings()
        {
        }

        public Boolean UseDataAnnotations { get; set; }

        public Boolean DeclareDbSetPropertiesInDbContext { get; set; }

        public Boolean DeclareNavigationPropertiesAsVirtual { get; set; }

        public String NavigationPropertyEnumerableNamespace { get; set; } = "System.Collections.ObjectModel";

        public String NavigationPropertyEnumerableType { get; set; } = "Collection";

        public String ConcurrencyToken { get; set; }

        public String EntityInterfaceName { get; set; } = "IEntity";

        public AuditEntity AuditEntity { get; set; }

        public Boolean GenerateTestsForRepositories { get; set; }

        private List<String> m_entitiesWithDataContracts;

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
    }
}
