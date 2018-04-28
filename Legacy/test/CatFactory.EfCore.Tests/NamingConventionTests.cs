using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestMapName()
        {
            // Arrange, Act and Assert
            Assert.True("OrderConfiguration" == new Table { Name = "Order" }.GetEntityTypeConfigurationName());
            Assert.True("OrdersQryConfiguration" == new View { Name = "Orders Qry" }.GetEntityTypeConfigurationName());
        }
    }
}
