using CatFactory.ObjectRelationalMapping;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestMapName()
        {
            // Arrange
            var efCoreProject = new EntityFrameworkCoreProject();

            // Act and Assert
            Assert.True("OrderConfiguration" == efCoreProject.GetEntityConfigurationName(new Table { Name = "Order" }));
            Assert.True("OrdersQryConfiguration" == efCoreProject.GetEntityConfigurationName(new View { Name = "Orders Qry" }));
        }
    }
}
