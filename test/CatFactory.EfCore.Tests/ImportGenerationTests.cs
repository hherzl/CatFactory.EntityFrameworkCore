using System.Collections.Generic;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class ImportGenerationTests
    {
        [Fact]
        public void ProjectScaffoldingFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\Store"
            };

            // Apply settings for Entity Framework Core project
            project.Settings.ForceOverwrite = true;
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithDataAnnotationsFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "StoreWithDataAnnotations",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\StoreWithDataAnnotations"
            };

            // Apply settings for Entity Framework Core project
            project.Settings.ForceOverwrite = true;
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");
            project.Settings.UseDataAnnotations = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithModifiedNamespacesFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\ModifiedNorthwind"
            };

            // Apply settings for Entity Framework Core project
            project.Settings.ForceOverwrite = true;

            // Set custom namespaces
            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";
            project.Namespaces.Contracts = "Interfaces";
            project.Namespaces.DataContracts = "Dtos";
            project.Namespaces.Repositories = "Implementations";

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
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\VsCode\\Northwind\\src"
            };

            // Apply settings for Entity Framework Core project
            project.Settings.ForceOverwrite = true;

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
            var factory = new SqlServerDatabaseFactory(LoggerMocker.GetLogger<SqlServerDatabaseFactory>())
            {
                ConnectionString = "server=(local);database=AdventureWorks2012;integrated security=yes;",
                ImportSettings = new DatabaseImportSettings
                {
                    Exclusions = new List<string>
                    {
                        "dbo.sysdiagrams"
                    }
                }
            };

            // Import database
            var database = factory.Import();

            // Create instance of Entity Framework Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\AdventureWorks"
            };

            // Apply settings for Entity Framework Core project
            project.Settings.ForceOverwrite = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
