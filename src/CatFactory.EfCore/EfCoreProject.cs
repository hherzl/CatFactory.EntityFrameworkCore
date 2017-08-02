using System.Collections.Generic;
using System.Diagnostics;
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

                    return new ProjectFeature(item, dbObjects)
                    {
                        Project = this
                    };
                })
                .ToList();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ProjectNamespaces m_namespaces;

        public ProjectNamespaces Namespaces
            => m_namespaces ?? (m_namespaces = new ProjectNamespaces());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EfCoreProjectSettings m_settings;

        public EfCoreProjectSettings Settings
            => m_settings ?? (m_settings = new EfCoreProjectSettings());
    }
}
