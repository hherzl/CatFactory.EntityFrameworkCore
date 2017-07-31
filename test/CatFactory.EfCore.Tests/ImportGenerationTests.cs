using System;
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

            // Create instance of Ef Core Project
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store"
            };

            // Set audit columns
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");

            // Set concurrency token
            project.Settings.ConcurrencyToken = "Timestamp";

            // Set the list of entities with 
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =D
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

            // Create instance of Ef Core Project
            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedNorthwind"
            };

            // Set custom namespaces
            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";
            project.Namespaces.Contracts = "Interfaces";
            project.Namespaces.DataContracts = "Dtos";
            project.Namespaces.Repositories = "Implementations";

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =D
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationForNorthwindDatabaseTest()
        {
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = "C:\\VsCode\\Northwind\\src"
            };

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationForAdventureWorksDatabaseTest()
        {
            var factory = new SqlServerDatabaseFactory(LoggerMocker.GetLogger<SqlServerDatabaseFactory>())
            {
                ConnectionString = "server=(local);database=AdventureWorks2012;integrated security=yes;",
                Exclusions = new List<String>()
                {
                    "dbo.sysdiagrams"
                }
            };

            var database = factory.Import();

            var project = new EfCoreProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\AdventureWorks"
            };

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
    }
}
