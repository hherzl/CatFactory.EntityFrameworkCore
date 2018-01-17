using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Mapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.EfCore
{
    public class EntityFrameworkCoreProject : Project<EntityFrameworkCoreProjectSettings>
    {
        public EntityFrameworkCoreProject()
        {
        }

        public EntityFrameworkCoreProject(ILogger<EntityFrameworkCoreProject> logger)
            : base(logger)
        {
        }

        public void Scaffolding(ICodeBuilder codeBuilder)
        {
            OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));
        }

        public void Scaffolded(ICodeBuilder codeBuilder)
        {
            OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
        }

        public override void BuildFeatures()
        {
            if (Database == null)
            {
                return;
            }

            if (this.GlobalSelection().Settings.AuditEntity != null)
            {
                this.GlobalSelection().Settings.EntityInterfaceName = "IAuditEntity";
            }

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item =>
                {
                    var dbObjects = GetDbObjects(Database, item);

                    return new ProjectFeature<EntityFrameworkCoreProjectSettings>(item, dbObjects)
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

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private EntityFrameworkCoreProjectSettings m_settings;

        //public EntityFrameworkCoreProjectSettings Settings
        //    => m_settings ?? (m_settings = new EntityFrameworkCoreProjectSettings());
    }
}
