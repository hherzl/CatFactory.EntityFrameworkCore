using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ProjectCreationTests
    {
        [Fact]
        public void DefineProjectForV2x()
        {
            var project = EntityFrameworkCoreProject.CreateForV2x("OnlineStore", new ObjectRelationalMapping.Database(), "");

            Assert.True(project.Version <= EntityFrameworkCoreVersion.Version_2_2);
        }

        [Fact]
        public void DefineProjectForV3x()
        {
            var project = EntityFrameworkCoreProject.CreateForV3x("OnlineStore", new ObjectRelationalMapping.Database(), "");

            Assert.True(project.Version >= EntityFrameworkCoreVersion.Version_3_0);
        }
    }
}
