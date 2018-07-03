using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class MockGenerationTests
    {
        [Fact]
        public void ProjectScaffoldingWithDefaultsFromClassicMockDatabaseTest()
        {
            // Create instance of EF Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "School",
                Database = Databases.School,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\School.Mock"
            };

            // Apply settings for EF Core project
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
