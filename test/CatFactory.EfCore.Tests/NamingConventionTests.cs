using System.Threading.Tasks;
using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public async Task TestSingular()
        {
            await Task.Run(() =>
            {
                Assert.True("Product" == new DbObject { Name = "Products" }.GetSingularName());
                Assert.True("Category" == new DbObject { Name = "Categories" }.GetSingularName());
            });
        }

        [Fact]
        public async Task TestPluralization()
        {
            await Task.Run(() =>
            {
                Assert.True("Queries" == new DbObject { Name = "Query" }.GetPluralName());
            });
        }

        [Fact]
        public async Task TestMapName()
        {
            await Task.Run(() =>
            {
                Assert.True("OrdersQryMap" == new View { Name = "Orders Qry" }.GetMapName());
            });
        }
    }
}
