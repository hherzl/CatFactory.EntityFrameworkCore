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
            Assert.True("OrderEntityTypeConfiguration" == new Table { Name = "Order" }.GetEntityTypeConfigurationName());
            Assert.True("OrdersQryEntityTypeConfiguration" == new View { Name = "Orders Qry" }.GetEntityTypeConfigurationName());
        }
    }
}
