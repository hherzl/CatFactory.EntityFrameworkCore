using System.Threading.Tasks;
using CatFactory.EntityFrameworkCore.Definitions;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public class ScaffoldingEventHandlersTests
    {
        [Fact]
        public async Task ScaffoldingWithEventHandlersForOnlineStoreDbAsync()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;"
                }
            };

            // Import database
            var database = await databaseFactory.ImportAsync();

            // Create instance of Entity Framework Core project
            var project = EntityFrameworkCoreProject
                .CreateForV2x("ScaffoldingEventHandlers.OnlineStore.Core", database, @"C:\Temp\CatFactory.EntityFrameworkCore\ScaffoldingEventHandlers.OnlineStore.Core");

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.ScaffoldingDefinition += (sender, args) =>
            {
                if (args.CodeBuilder.ObjectDefinition is EntityClassDefinition entityDef)
                {
                    foreach (var prop in entityDef.Properties)
                    {
                        if (prop.Type == "string")
                            prop.Type = "string?";
                    }
                }
                else if (args.CodeBuilder.ObjectDefinition is DbContextClassDefinition dbContextDef)
                {
                    dbContextDef.Documentation.Summary = "foo bar zaz";

                    foreach (var prop in dbContextDef.Properties)
                    {
                        // todo: Add 
                        prop.InitializationValue = "null";
                    }
                }
            };

            project.ScaffoldedDefinition += (sender, args) =>
            {
                // Check if file exists
            };

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldDomain();
        }
    }
}
