using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestSingular()
        {
            Assert.True("Product" == new DbObject { Name = "Products" }.GetSingularName());
            Assert.True("Category" == new DbObject { Name = "Categories" }.GetSingularName());
        }

        [Fact]
        public void TestPluralization()
        {
            Assert.True("Queries" == new DbObject { Name = "Query" }.GetPluralName());
        }

        [Fact]
        public void TestMapName()
        {
            Assert.True("OrdersQryMap" == new View { Name = "Orders Qry" }.GetMapName());
        }
    }
}
