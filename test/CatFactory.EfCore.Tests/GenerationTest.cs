using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class GenerationTest
    {
        [Fact]
        public void ProjectGenerationWithDefaultsFromMockDatabaseTest()
        {
            var project = new EfCoreProject()
            {
                Name = "Sales",
                Database = Mocks.SalesDatabase,
                OutputDirectory = "C:\\Temp\\Sales"
            };

            project.BuildFeatures();

            project
                .GenerateEntities()
                .GenerateAppSettings()
                .GenerateMappingDependences()
                .GenerateMappings()
                .GenerateDbContext()
                .GenerateContracts()
                .GenerateRepositories();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromMockDatabaseTest()
        {
            var project = new EfCoreProject()
            {
                Name = "Sales",
                Database = Mocks.SalesDatabase,
                OutputDirectory = "C:\\Temp\\ModifiedSales"
            };

            project.BuildFeatures();

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            project
                .GenerateEntities()
                .GenerateAppSettings()
                .GenerateMappingDependences()
                .GenerateMappings()
                .GenerateDbContext()
                .GenerateContracts()
                .GenerateRepositories();
        }

        [Fact]
        public void ProjectGenerationWithModifiedNamespacesFromExistingDatabaseTest()
        {
            var connectionString = "server=(local);database=Northwind;integrated security=yes;";

            var dbFactory = new SqlServerDatabaseFactory()
            {
                ConnectionString = connectionString
            };

            var db = dbFactory.Import();

            var project = new EfCoreProject()
            {
                Name = "Northwind",
                Database = db,
                OutputDirectory = "C:\\Temp\\ModifiedNorthwind"
            };

            project.BuildFeatures();

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            project
                .GenerateEntities()
                .GenerateAppSettings()
                .GenerateMappingDependences()
                .GenerateMappings()
                .GenerateDbContext()
                .GenerateContracts()
                .GenerateRepositories();
        }

        [Fact]
        public void ProjectGenerationFromExistingDatabaseTest()
        {
            var connectionString = "server=(local);database=Northwind;integrated security=yes;";

            var dbFactory = new SqlServerDatabaseFactory()
            {
                ConnectionString = connectionString
            };

            var db = dbFactory.Import();

            var project = new EfCoreProject()
            {
                Name = "Northwind",
                Database = db,
                OutputDirectory = "C:\\VsCode\\Northwind\\src"
            };

            project.BuildFeatures();

            project
                .GenerateEntities()
                .GenerateAppSettings()
                .GenerateMappingDependences()
                .GenerateMappings()
                .GenerateDbContext()
                .GenerateContracts()
                .GenerateRepositories();
        }
    }
}
