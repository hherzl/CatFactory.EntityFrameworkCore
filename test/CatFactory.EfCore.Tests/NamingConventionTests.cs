using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestSingular()
        {
            // Arrange, Act and Assert
            Assert.True("Category" == new DbObject { Name = "Categories" }.GetSingularName());
            Assert.True("Product" == new DbObject { Name = "Products" }.GetSingularName());

        }

        [Fact]
        public void TestPlural()
        {
            // Arrange, Act and Assert
            Assert.True("Addresses" == new DbObject { Name = "Address" }.GetPluralName());
            Assert.True("Books" == new DbObject { Name = "Book" }.GetPluralName());
            Assert.True("Queries" == new DbObject { Name = "Query" }.GetPluralName());
        }

        [Fact]
        public void TestMapName()
        {
            // Arrange, Act and Assert
            Assert.True("OrderMap" == new Table { Name = "Order" }.GetMapName());
            Assert.True("OrdersQryMap" == new View { Name = "Orders Qry" }.GetMapName());
        }
    }
}
