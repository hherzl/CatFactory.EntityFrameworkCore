using System.Threading.Tasks;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ProjectSelectionTests
    {
        [Fact]
        public async Task ProjectSelectionScopeAsync()
        {
            // Arrange

            // Import database
            var database = await SqlServerDatabaseFactory
                .ImportAsync("server=(local); database=OnlineStore; integrated security=yes; TrustServerCertificate=True;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("OnlineStore", database, @"C:\Temp\CatFactory.EntityFrameworkCore\OnlineStore");

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
