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
            var db = SqlServerDatabaseFactory
                .Import("server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Ef Core Project
            var project = new EfCoreProject()
            {
                Name = "Store",
                Database = db,
                OutputDirectory = "C:\\Temp\\Store"
            };

            // Set audit columns
            project.Settings.AuditEntity = new AuditEntity
            {
                CreationUserColumnName = "CreationUser",
                CreationDateTimeColumnName = "CreationDateTime",
                LastUpdateUserColumnName = "LastUpdateUser",
                LastUpdateDateTimeColumnName = "LastUpdateDateTime"
            };

            // Set concurrency token
            project.Settings.ConcurrencyToken = "Timestamp";

            // Set the list of entities with 
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code :=)
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromExistingDatabaseTest()
        {
            var db = SqlServerDatabaseFactory.Import("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = db,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedNorthwind"
            };

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationForNorthwindDatabaseTest()
        {
            var db = SqlServerDatabaseFactory.Import("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject
            {
                Name = "Northwind",
                Database = db,
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
            var db = SqlServerDatabaseFactory.Import("server=(local);database=AdventureWorks2012;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject
            {
                Name = "AdventureWorks",
                Database = db,
                OutputDirectory = "C:\\VsCode\\AdventureWorks\\src"
            };

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
    }
}
