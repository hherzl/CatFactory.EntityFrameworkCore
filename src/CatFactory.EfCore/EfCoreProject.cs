using System.Collections.Generic;
using System.Diagnostics;
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
                    var dbObjects = GetDbObjects(Database, item);

                    return new ProjectFeature(item, dbObjects)
                    {
                        Project = this
                    };
                })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            var result = new List<DbObject>();

            result.AddRange(Database
                .Tables
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "USER_TABLE" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "VIEW" }));

            return result;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_updateExclusions;

        public List<string> UpdateExclusions
        {
            get
            {
                return m_updateExclusions ?? (m_updateExclusions = new List<string>());
            }
            set
            {
                m_updateExclusions = value;
            }
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
