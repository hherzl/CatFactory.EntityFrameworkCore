using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ProjectSelectionTests
    {
        [Fact]
        public void TestProjectSelectionScope()
        {
            // Arrange

            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerHelper.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = new EntityFrameworkCoreProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\Store"
            };

            // Act

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
                settings.ConcurrencyToken = "Timestamp";
            });

            project.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

            var order = database.FindTable("Sales.Order");

            var selectionForOrder = project.GetSelection(order);

            // Assert

            Assert.True(project.Selections.Count == 2);

            Assert.True(project.GlobalSelection().Settings.EntitiesWithDataContracts == false);
            Assert.True(selectionForOrder.Settings.EntitiesWithDataContracts == true);
        }
    }
}
