using System.Linq;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class GenerationTest
    {
        [Fact]
        public void ProjectGenerationWithDefaultsFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = Mocks.StoreDatabase,
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
                Database = Mocks.StoreDatabase,
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
                Database = Mocks.StoreDatabase,
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
            var connectionString = "server=(local);database=Northwind;integrated security=yes;";

            var dbFactory = new SqlServerDatabaseFactory
            {
                ConnectionString = connectionString
            };

            var db = dbFactory.Import();

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
        public void ProjectGenerationFromExistingDatabaseTest()
        {
            var connectionString = "server=(local);database=Northwind;integrated security=yes;";

            var dbFactory = new SqlServerDatabaseFactory
            {
                ConnectionString = connectionString
            };

            dbFactory.Exclusions.Add("dbo.sysdiagrams");

            var db = dbFactory.Import();

            var dbObject = db.DbObjects.FirstOrDefault(item => item.FullName == "dbo.sysdiagrams");

            if (dbObject != null)
            {
                db.DbObjects.Remove(dbObject);
            }

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
    }
}
