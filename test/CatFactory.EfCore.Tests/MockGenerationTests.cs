using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class MockGenerationTests
    {
        [Fact]
        public void ProjectScaffoldingWithDefaultsFromMockDatabaseTest()
        {
            // Create instance of EF Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = Databases.Store,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\Store.Mock"
            };

            // Apply settings for EF Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
                settings.ConcurrencyToken = "Timestamp";
            });

            project.Select("Sales.Order", settings =>
            {
                settings.ForceOverwrite = true;
                settings.EntitiesWithDataContracts = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithDataBindingsFromMockDatabaseTest()
        {
            // Create instance of EF Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = Databases.Store,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\Store.Mock.DataBindings"
            };

            // Apply settings for EF Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.EnableDataBindings = true;
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
                settings.ConcurrencyToken = "Timestamp";
            });

            project.Select("Sales.Order", settings =>
            {
                settings.ForceOverwrite = true;
                settings.EntitiesWithDataContracts = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

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
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithDbSetPropertiesAndDataAnnotationsTest()
        {
            // Create instance of EF Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = Databases.Store,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\StoreWithDbSetPropertiesAndDataAnnotations.Mock"
            };

            // Apply settings for EF Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UseDataAnnotations = true;
                settings.DeclareDbSetPropertiesInDbContext = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingWithModifiedNamespacesFromMockDatabaseTest()
        {
            // Create instance of EF Core Project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = Databases.Store,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\ModifiedStore.Mock"
            };

            // Apply settings for EF Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
