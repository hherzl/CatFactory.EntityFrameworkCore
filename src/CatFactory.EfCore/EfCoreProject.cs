using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.Mapping;
using CatFactory.SqlServer;

namespace CatFactory.EfCore
{
    public class EfCoreProject : Project
    {
        public EfCoreProject()
        {
        }

        private ProjectNamespaces m_namespaces;

        public ProjectNamespaces Namespaces
        {
            get
            {
                return m_namespaces ?? (m_namespaces = new ProjectNamespaces());
            }
            set
            {
                m_namespaces = value;
            }
        }

        public Boolean UseDataAnnotations { get; set; }

        public Boolean DeclareDbSetPropertiesInDbContext { get; set; }

        public Boolean DeclareNavigationPropertiesAsVirtual { get; set; }

        public String NavigationPropertyEnumerableNamespace { get; set; } = "System.Collections.ObjectModel";

        public String NavigationPropertyEnumerableType { get; set; } = "Collection";

        public String ConcurrencyToken { get; set; }

        public String EntityInterfaceName { get; set; } = "IEntity";

        public AuditEntity AuditEntity { get; set; }

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

        public override void BuildFeatures()
        {
            if (Database == null)
            {
                return;
            }

            if (AuditEntity != null)
            {
                EntityInterfaceName = "IAuditEntity";
            }

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item =>
                {
                    var dbObjects = new List<DbObject>();

                    dbObjects.AddRange(Database.GetTables().Where(t => t.Schema == item));
                    dbObjects.AddRange(Database.GetViews().Where(v => v.Schema == item));

                    return new ProjectFeature(item, dbObjects, Database);
                })
                .ToList();
        }
    }
}
