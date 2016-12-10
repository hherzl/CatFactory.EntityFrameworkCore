using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class GenerationTest
    {
        [Fact]
        public void ProjectGenerationFromMockDatabaseTest()
        {
            var project = new EfCoreProject()
            {
                Name = "Sales",
                Database = Mocks.SalesDatabase,
                OutputDirectory = "C:\\Temp"
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
