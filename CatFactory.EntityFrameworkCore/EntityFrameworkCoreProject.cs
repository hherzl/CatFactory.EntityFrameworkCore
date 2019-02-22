using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore.Definitions;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
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
                .Select(item => new ProjectFeature<EntityFrameworkCoreProjectSettings>(item, GetDbObjects(Database, item), this))
                .ToList();
        }

        public void Scaffold(IObjectDefinition objectDefinition, string outputDirectory, string subdirectory)
        {
            ICodeBuilder codeBuilder = null;

            var selection = this.GetSelection(objectDefinition.DbObject);

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

            codeBuilder.CreateFile();

            OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
        }

        public override void Scaffold()
        {
            foreach (var definition in ObjectDefinitions)
            {
                ICodeBuilder codeBuilder = null;

                if (definition is AuditEntityInterfaceDefinition)
                {
                    codeBuilder = new CSharpInterfaceBuilder
                    {
                        OutputDirectory = this.GetEntityLayerDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }

                else if (definition is EntityInterfaceDefinition)
                {
                    codeBuilder = new CSharpInterfaceBuilder
                    {
                        OutputDirectory = this.GetEntityLayerDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is EntityClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetEntityLayerDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile(Database.HasDefaultSchema(definition.DbObject) ? "" : definition.DbObject.Schema);

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is EntityConfigurationClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerConfigurationsDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile(Database.HasDefaultSchema(definition.DbObject) ? "" : definition.DbObject.Schema);

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is DbContextClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is CSharpInterfaceDefinition)
                {
                    codeBuilder = new CSharpInterfaceBuilder
                    {
                        OutputDirectory = this.GetDataLayerContractsDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is DataContractClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerDataContractsDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is RepositoryExtensionsClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerRepositoriesDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is RepositoryBaseClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerRepositoriesDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
                else if (definition is RepositoryClassDefinition)
                {
                    codeBuilder = new CSharpClassBuilder
                    {
                        OutputDirectory = this.GetDataLayerRepositoriesDirectory(),
                        ForceOverwrite = this.GlobalSelection().Settings.ForceOverwrite,
                        ObjectDefinition = definition
                    };

                    OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

                    codeBuilder.CreateFile();

                    OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
                }
            }
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            foreach (var table in Database.Tables.Where(item => item.Schema == schema))
            {
                yield return new DbObject(table.Schema, table.Name)
                {
                    Type = "Table"
                };
            }

            foreach (var view in Database.Views.Where(item => item.Schema == schema))
            {
                yield return new DbObject(view.Schema, view.Name)
                {
                    Type = "View"
                };
            }

            foreach (var scalarFunction in Database.ScalarFunctions.Where(item => item.Schema == schema))
            {
                yield return new DbObject(scalarFunction.Schema, scalarFunction.Name)
                {
                    Type = "ScalarFunction"
                };
            }

            foreach (var tableFunction in Database.TableFunctions.Where(x => x.Schema == schema))
            {
                yield return new DbObject(tableFunction.Schema, tableFunction.Name)
                {
                    Type = "TableFunction"
                };
            }

            foreach (var storedProcedure in Database.StoredProcedures.Where(item => item.Schema == schema))
            {
                yield return new DbObject(storedProcedure.Schema, storedProcedure.Name)
                {
                    Type = "StoredProcedure"
                };
            }
        }
    }
}
