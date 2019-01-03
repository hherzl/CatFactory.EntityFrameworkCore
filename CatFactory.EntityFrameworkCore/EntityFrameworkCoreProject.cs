using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.EntityFrameworkCore
{
    public class EntityFrameworkCoreProject : Project<EntityFrameworkCoreProjectSettings>
    {
        public EntityFrameworkCoreProject()
            : base()
        {
            CodeNamingConvention = new DotNetNamingConvention();
            NamingService = new NamingService();
        }

        public EntityFrameworkCoreProject(ILogger<EntityFrameworkCoreProject> logger)
            : base(logger)
        {
            CodeNamingConvention = new DotNetNamingConvention();
            NamingService = new NamingService();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EntityFrameworkCoreProjectNamespaces m_projectNamespaces;

        public EntityFrameworkCoreProjectNamespaces ProjectNamespaces
        {
            get
            {
                return m_projectNamespaces ?? (m_projectNamespaces = new EntityFrameworkCoreProjectNamespaces());
            }
            set
            {
                m_projectNamespaces = value;
            }
        }

        // todo: Add logic to show author's info
        public AuthorInfo AuthorInfo { get; set; }

        public override void BuildFeatures()
        {
            if (Database == null)
                return;

            if (this.GlobalSelection().Settings.AuditEntity != null)
                this.GlobalSelection().Settings.EntityInterfaceName = "IAuditEntity";

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature<EntityFrameworkCoreProjectSettings>(item, GetDbObjects(Database, item)) { Project = this })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            var result = new List<DbObject>();

            result.AddRange(Database
                .Tables
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "Table" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "View" }));

            result.AddRange(Database
                .ScalarFunctions
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "ScalarFunction" }));

            result.AddRange(Database
                .TableFunctions
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "TableFunction" }));

            result.AddRange(Database
                .StoredProcedures
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "StoredProcedure" }));

            return result;
        }
    }
}
