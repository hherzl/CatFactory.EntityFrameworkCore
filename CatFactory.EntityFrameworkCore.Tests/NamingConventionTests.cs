using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestMapName()
        {
            // Arrange, Act and Assert
            Assert.True("OrderConfiguration" == new Table { Name = "Order" }.GetEntityConfigurationName());
            Assert.True("OrdersQryConfiguration" == new View { Name = "Orders Qry" }.GetEntityConfigurationName());
        }
    }
}
