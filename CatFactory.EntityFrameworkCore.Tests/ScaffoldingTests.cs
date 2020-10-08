using System.Threading.Tasks;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ScaffoldingTests
    {
        [Fact]
        public async Task Foo()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
                    Exclusions =
                    {
                        "dbo.sysdiagrams"
                    }
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV3x("OnlineStore.Domain", database, @"C:\Temp\CatFactory.EntityFrameworkCore\OnlineStore2.Domain");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };

                settings.AddConfigurationForUniquesInFluentAPI = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            project.Selection("Sales.OrderHeader", settings =>
            {
                settings.EntitiesWithDataContracts = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldDomain();
        }

        [Fact]
        public async Task ScaffoldingDomainProjectForOnlineStoreDbAsync()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
                    Exclusions =
                    {
                        "dbo.sysdiagrams"
                    }
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("OnlineStore.Domain", database, @"C:\Temp\CatFactory.EntityFrameworkCore\OnlineStore.Domain");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };

                settings.AddConfigurationForUniquesInFluentAPI = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            project.Selection("Sales.OrderHeader", settings =>
            {
                settings.EntitiesWithDataContracts = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldDomain();
        }

        [Fact]
        public async Task ScaffoldingProjectWithRepositoriesForOnlineStoreDbAsync()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("OnlineStore.Core", database, @"C:\Temp\CatFactory.EntityFrameworkCore\OnlineStore.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };

                settings.AddConfigurationForUniquesInFluentAPI = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            project.Selection("Sales.OrderHeader", settings =>
            {
                settings.EntitiesWithDataContracts = true;
                settings.AddConfigurationForForeignKeysInFluentAPI = true;
                settings.DeclareNavigationProperties = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public async Task ScaffoldingProjectWithDataAnnotationsForOnlineStoreDbAsync()
        {
            // Import database
            var database = await SqlServerDatabaseFactory
                .ImportAsync("server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("OnlineStoreWithDataAnnotations.Core", database, @"C:\Temp\CatFactory.EntityFrameworkCore\OnlineStoreWithDataAnnotations.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UseDataAnnotations = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };
                settings.DeclareNavigationProperties = true;
            });

            project.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public async Task ScaffoldingProjectForNorthwindDbAsync()
        {
            // Import database
            var factory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=Northwind;integrated security=yes;",
                    ImportScalarFunctions = true,
                    ImportTableFunctions = true,
                    ImportStoredProcedures = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.sp_alterdiagram",
                        "dbo.sp_creatediagram",
                        "dbo.sp_dropdiagram",
                        "dbo.sp_helpdiagramdefinition",
                        "dbo.sp_helpdiagrams",
                        "dbo.sp_renamediagram",
                        "dbo.sp_upgraddiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            var database = await factory.ImportAsync();

            // Create instance of Entity Framework Core Project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("Northwind.Core", database, @"C:\Temp\CatFactory.EntityFrameworkCore\Northwind.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.DeclareNavigationProperties = true;
            });

            project.Selection("dbo.Orders", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public async Task ScaffoldingProjectForAdventureWorksDbAsync()
        {
            // Create instance of factory for SQL Server
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=AdventureWorks2017;integrated security=yes;",
                    ImportScalarFunctions = true,
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "Production.Document",
                        "Production.ProductDocument"
                    },
                    ExclusionTypes =
                    {
                        "hierarchyid",
                        "geography"
                    }
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core Project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("AdventureWorks", database, @"C:\Temp\CatFactory.EntityFrameworkCore\AdventureWorks.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.DeclareNavigationProperties = true;

                settings.AddConfigurationForForeignKeysInFluentAPI = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public async Task ScaffoldingProjectForWideWorldImportersDbAsync()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=WideWorldImporters;integrated security=yes;",
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("WideWorldImporters.Core", database, @"C:\Temp\CatFactory.EntityFrameworkCore\WideWorldImporters.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
