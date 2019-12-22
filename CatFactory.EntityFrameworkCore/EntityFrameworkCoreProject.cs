using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.EntityFrameworkCore
{
    public class EntityFrameworkCoreProject : CSharpProject<EntityFrameworkCoreProjectSettings>
    {
        public static EntityFrameworkCoreProject Create(string name, Database database, string outputDirectory)
            => new EntityFrameworkCoreProject
            {
                Name = name,
                Database = database,
                OutputDirectory = outputDirectory
            };

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EntityFrameworkCoreProjectNamespaces m_projectNamespaces;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Dictionary<string, Type> m_valueConversionMaps;

        public EntityFrameworkCoreProject()
            : base()
        {
        }

        public EntityFrameworkCoreProject(ILogger<EntityFrameworkCoreProject> logger)
            : base(logger)
        {
        }

        public EntityFrameworkCoreProjectNamespaces ProjectNamespaces
        {
            get => m_projectNamespaces ?? (m_projectNamespaces = new EntityFrameworkCoreProjectNamespaces());
            set => m_projectNamespaces = value;
        }

        /// <summary>
        /// A dictionary of (string)CatFactory.ObjectRelationalMapping.DatabaseTypeMap.DatabaseType to
        /// {OutputDirectory}\{EntityFrameworkCoreProjectNamespaces.ValueConversions}\Type can be
        /// submitted to the Entity Framework Core project via ValueConversionMaps for use in {Enity}Configuration.cs
        /// files
        /// </summary>
        public Dictionary<string, Type> ValueConversionMaps
        {
            get => m_valueConversionMaps ?? (m_valueConversionMaps = new Dictionary<string, Type>());
            set => m_valueConversionMaps = value;
        }

        // todo: Add logic to show author's info
        public AuthorInfo AuthorInfo { get; set; }

        protected override IEnumerable<DbObject> GetDbObjectsBySchema(string schema)
        {
            foreach (var item in base.GetDbObjectsBySchema(schema))
            {
                yield return item;
            }

            foreach (var item in Database.GetTableFunctions().Where(tableFunction => tableFunction.Schema == schema))
            {
                yield return new DbObject(item.Schema, item.Name)
                {
                    Type = "TableFunction"
                };
            }

            foreach (var item in Database.GetStoredProcedures().Where(storedProcedure => storedProcedure.Schema == schema))
            {
                yield return new DbObject(item.Schema, item.Name)
                {
                    Type = "StoredProcedure"
                };
            }
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
                .Select(item => new ProjectFeature<EntityFrameworkCoreProjectSettings>(item, GetDbObjectsBySchema(item), this))
                .ToList();
        }

        public override void Scaffold(IObjectDefinition objectDefinition, string outputDirectory, string subdirectory = "")
        {
            var codeBuilder = default(ICodeBuilder);

            var selection = objectDefinition.DbObject == null ? this.GlobalSelection() : this.GetSelection(objectDefinition.DbObject);

            if (objectDefinition is CSharpClassDefinition)
            {
                codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }
            else if (objectDefinition is CSharpInterfaceDefinition)
            {
                codeBuilder = new CSharpInterfaceBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }

            OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

            codeBuilder.CreateFile(subdirectory: subdirectory);

            OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
        }
    }
}
