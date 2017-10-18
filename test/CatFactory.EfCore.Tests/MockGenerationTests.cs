using System;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class MockGenerationTests
    {
        [Fact]
        public void ProjectGenerationWithDefaultsFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store.Mock"
            };

            project.Settings.ForceOverwrite = true;

            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithDataBindingsFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store.Mock.DataBindings"
            };

            project.Settings.ForceOverwrite = true;

            project.Settings.EnableDataBindings = true; 
            project.Settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
            project.Settings.ConcurrencyToken = "Timestamp";
            project.Settings.EntitiesWithDataContracts.Add("Sales.Order");

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationWithDefaultsFromClassicMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "School",
                Database = SchoolDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\School.Mock"
            };

            project.Settings.ForceOverwrite = true;

            project.BuildFeatures();

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
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\StoreWithDbSetPropertiesAndDataAnnotations.Mock"
            };

            project.Settings.ForceOverwrite = true;

            project.Settings.UseDataAnnotations = true;
            project.Settings.DeclareDbSetPropertiesInDbContext = true;

            project.BuildFeatures();

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
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\ModifiedStore.Mock"
            };

            project.Settings.ForceOverwrite = true;

            project.Namespaces.EntityLayer = "EL";
            project.Namespaces.DataLayer = "DL";

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
        
        [Fact]
        public void ProjectGenerationWithTddFromMockDatabaseTest()
        {
            var project = new EfCoreProject
            {
                Name = "Store",
                Database = StoreDatabase.Mock,
                OutputDirectory = "C:\\Temp\\CatFactory.EfCore\\Store.Tdd.Mock",
            };

            project.Settings.ForceOverwrite = true;

            // todo: add logic to this feature
            //project.Settings.GenerateTestsForRepositories = true;

            project.UpdateExclusions.AddRange(new String[] { "CreationUser", "CreationDateTime" });

            project.Settings.AuditEntity = new AuditEntity
            {
                CreationUserColumnName = "CreationUser",
                CreationDateTimeColumnName = "CreationDateTime",
                LastUpdateUserColumnName = "LastUpdateUser",
                LastUpdateDateTimeColumnName = "LastUpdateDateTime"
            };

            project.Settings.ConcurrencyToken = "Timestamp";

            project.BuildFeatures();

            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
    }
}
