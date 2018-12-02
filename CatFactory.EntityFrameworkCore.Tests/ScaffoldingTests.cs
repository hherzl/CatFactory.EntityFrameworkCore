using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ScaffoldingTests
    {
        [Fact]
        public void ProjectScaffoldingForStoreDatabaseTest()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnLineStore;integrated security=yes;",
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            // Import database
            var database = databaseFactory.Import();

            // Create instance of Entity Framework Core project
            var project = new EntityFrameworkCoreProject
            {
                Name = "OnLineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.EntityFrameworkCore\OnLineStore.Core"
            };

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
                settings.ConcurrencyToken = "Timestamp";
            });

            project.Select("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Add event handlers to before and after of scaffold

            project.ScaffoldingDefinition += (source, args) =>
            {
                // Add code to perform operations with code builder instance before to create code file
            };

            project.ScaffoldedDefinition += (source, args) =>
            {
                // Add code to perform operations after of create code file
            };

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithDataAnnotationsForStoreDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=OnLineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "OnLineStoreWithDataAnnotations.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.EntityFrameworkCore\OnLineStoreWithDataAnnotations.Core"
            };

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
                settings.ConcurrencyToken = "Timestamp";
                settings.UseDataAnnotations = true;
            });

            project.Select("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingForNorthwindDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Northwind.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.EntityFrameworkCore\Northwind.Core"
            };

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Select("dbo.Orders", settings => settings.EntitiesWithDataContracts = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingForAdventureWorksDatabaseTest()
        {
            // Create instance of factory for SQL Server
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
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
            var database = databaseFactory.Import();

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.EntityFrameworkCore\AdventureWorks.Core"
            };

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
