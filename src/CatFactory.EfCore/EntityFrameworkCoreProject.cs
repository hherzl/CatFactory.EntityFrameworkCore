using System.Collections.Generic;
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
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "USER_TABLE" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "VIEW" }));

            return result;
        }

        public ProjectNamespaces Namespaces { get; set; } = new ProjectNamespaces();

        // todo: add logic to show author's info
        public AuthorInfo AuthorInfo { get; set; }
    }
}
