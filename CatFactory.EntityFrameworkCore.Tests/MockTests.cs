using CatFactory.EntityFrameworkCore.Tests.Models;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class MockTests
    {
        [Fact]
        public void ProjectScaffoldingForMockDb()
        {
            // Create instance of EF Core Project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("College.Mock", Databases.College, @"C:\Temp\CatFactory.EntityFrameworkCore\College.Mock");

            // Apply settings for EF Core project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer()
                ;
        }
    }
}
