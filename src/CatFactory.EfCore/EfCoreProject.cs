using System.Collections.Generic;
using System.Linq;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public class EfCoreProject : Project
    {
        public EfCoreProject()
        {
        }

        public override void BuildFeatures()
        {
            if (Database == null)
            {
                return;
            }

            if (Settings.AuditEntity != null)
            {
                Settings.EntityInterfaceName = "IAuditEntity";
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

        private ProjectNamespaces m_namespaces;
        private EfCoreProjectSettings m_settings;

        public ProjectNamespaces Namespaces
        {
            get
            {
                return m_namespaces ?? (m_namespaces = new ProjectNamespaces());
            }
        }

        public EfCoreProjectSettings Settings
        {
            get
            {
                return m_settings ?? (m_settings = new EfCoreProjectSettings());
            }
        }
    }
}
