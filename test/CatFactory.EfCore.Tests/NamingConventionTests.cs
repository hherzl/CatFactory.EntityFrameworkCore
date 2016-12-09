using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using Xunit;

namespace CatFactory.EfCore.Tests
{
    public class NamingConventionTests
    {
        [Fact]
        public void TestMapName()
        {
            var namingConvention = new DotNetNamingConvention() as INamingConvention;

            var view = new View { Name = "Orders Qry" };

            Assert.True("OrdersQryMap" == view.GetMapName());
        }
    }
}
