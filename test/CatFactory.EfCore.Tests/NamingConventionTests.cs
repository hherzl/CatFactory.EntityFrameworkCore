using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestSingular()
        {
            var dbObject = new DbObject { Name = "Products" };

            Assert.True("Product" == dbObject.GetSingularName());
        }

        [Fact]
        public void TestPluralization()
        {
            var dbObject = new DbObject { Name = "Query" };

            Assert.True("Queries" == dbObject.GetPluralName());
        }

        [Fact]
        public void TestMapName()
        {
            var namingConvention = new DotNetNamingConvention() as INamingConvention;

            var view = new View { Name = "Orders Qry" };

            Assert.True("OrdersQryMap" == view.GetMapName());
        }
    }
}
