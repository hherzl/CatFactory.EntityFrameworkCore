using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class GenerationTest
    {
        [Fact]
        public void ProjectGenerationWithDefaultsFromClassicMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "School",
                Database = SchoolDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\School"
            };

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithDefaultsFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store"
            };

            project.BuildFeatures();

            project.UpdateExclusions.Add("CreationUser");
            project.UpdateExclusions.Add("CreationDateTime");

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithDbSetPropertiesAndDataAnnotationsTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\StoreWithDbSetPropertiesAndDataAnnotations"
            };

            project.BuildFeatures();

            project.UseDataAnnotations = true;
            project.DeclareDbSetPropertiesInDbContext = true;

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedStore"
            };

            project.BuildFeatures();

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromExistingDatabaseTest()
        {
            var db = SqlServerDatabaseFactory.Import("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            var project = new EfCoreProject()
            {
                Name = "Northwind",
                Database = db,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedNorthwind"
            };

            project.BuildFeatures();

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

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
