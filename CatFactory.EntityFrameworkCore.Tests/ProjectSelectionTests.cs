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
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=OnLineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = new EntityFrameworkCoreProject
            {
                Name = "OnLineStore",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.EntityFrameworkCore\\OnLineStore"
            };

            // Act

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.ConcurrencyToken = "Timestamp";
                settings.AuditEntity = new AuditEntity
                {
                    CreationUserColumnName = "CreationUser",
                    CreationDateTimeColumnName = "CreationDateTime",
                    LastUpdateUserColumnName = "LastUpdateUser",
                    LastUpdateDateTimeColumnName = "LastUpdateDateTime"
                };
            });

            project.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

            var orderHeader = database.FindTable("Sales.OrderHeader");

            var selectionForOrder = project.GetSelection(orderHeader);

            // Assert

            Assert.True(project.Selections.Count == 2);

            Assert.True(project.GlobalSelection().Settings.EntitiesWithDataContracts == false);
            Assert.True(selectionForOrder.Settings.EntitiesWithDataContracts == true);
        }
    }
}
