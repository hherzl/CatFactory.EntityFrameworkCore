using System.Collections.Generic;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class ImportGenerationTests
    {
        [Fact]
        public void ProjectGenerationFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of EF Core Project
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store"
            };

            // Apply settings for EF Core project
            project.Settings.ForceOverwrite = true;
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of EF Core Project
            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedNorthwind"
            };

            // Apply settings for EF Core project
            project.Settings.ForceOverwrite = true;

            // Set custom namespaces
            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";
            project.Namespaces.Contracts = "Interfaces";
            project.Namespaces.DataContracts = "Dtos";
            project.Namespaces.Repositories = "Implementations";

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationForNorthwindDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of EF Core Project
            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\VsCode\\Northwind\\src"
            };

            // Apply settings for EF Core project
            project.Settings.ForceOverwrite = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationForAdventureWorksDatabaseTest()
        {
            // Create instance of factory for SQL Server
            var factory = new SqlServerDatabaseFactory(LoggerMocker.GetLogger<SqlServerDatabaseFactory>())
            {
                ConnectionString = "server=(local);database=AdventureWorks2012;integrated security=yes;",
                Exclusions = new List<string>()
                {
                    "dbo.sysdiagrams"
                }
            };

            // Import database
            var database = factory.Import();

            // Create instance of EF Core Project
            var project = new EfCoreProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\AdventureWorks"
            };

            // Apply settings for EF Core project
            project.Settings.ForceOverwrite = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
    }
}
