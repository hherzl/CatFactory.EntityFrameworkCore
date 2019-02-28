using System.Collections.Generic;
using System.IO;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DataLayerExtensions
    {
        public static EntityFrameworkCoreProject ScaffoldDataLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldConfigurations(project);
            ScaffoldDbContext(project);
            ScaffoldDataContracts(project);
            ScaffoldDataRepositories(project);
            ScaffoldReadMe(project);

            return project;
        }

        private static void ScaffoldConfigurations(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    var definition = project.GetEntityConfigurationClassDefinition(table);

                    project.Scaffold(definition, project.GetDataLayerConfigurationsDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
                }

                foreach (var view in project.Database.Views)
                {
                    var definition = project.GetEntityConfigurationClassDefinition(view);

                    project.Scaffold(definition, project.GetDataLayerConfigurationsDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
                }
            }
        }

        private static void ScaffoldDbContext(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();
            
            project.Scaffold(project.GetDbContextClassDefinition(projectSelection), project.GetDataLayerDirectory());
        }

        private static void ScaffoldRepositoryInterface(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryInterfaceDefinition(), project.GetDataLayerContractsDirectory());
        }

        private static void ScaffoldBaseRepositoryClassDefinition(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryBaseClassDefinition(), project.GetDataLayerRepositoriesDirectory());
        }

        private static void ScaffoldRepositoryExtensionsClassDefinition(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryExtensionsClassDefinition(), project.GetDataLayerRepositoriesDirectory());
        }

        private static void ScaffoldDataContracts(EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                if (!selection.Settings.EntitiesWithDataContracts)
                    continue;

                project.Scaffold(project.GetDataContractClassDefinition(table), project.GetDataLayerDataContractsDirectory());
            }
        }

        private static void ScaffoldDataRepositories(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken))
            {
                projectSelection.Settings.InsertExclusions.Add(projectSelection.Settings.ConcurrencyToken);
                projectSelection.Settings.UpdateExclusions.Add(projectSelection.Settings.ConcurrencyToken);
            }

            ScaffoldRepositoryInterface(project);
            ScaffoldBaseRepositoryClassDefinition(project);
            ScaffoldRepositoryExtensionsClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                project.Scaffold(repositoryClassDefinition, project.GetDataLayerRepositoriesDirectory());

                var repositoryInterfaceDefinition = repositoryClassDefinition.RefactInterface();

                repositoryInterfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();
                repositoryInterfaceDefinition.Implements.Add("IRepository");

                project.Scaffold(repositoryInterfaceDefinition, project.GetDataLayerContractsDirectory());
            }
        }

        private static void ScaffoldReadMe(this EntityFrameworkCoreProject project)
        {
            var lines = new List<string>
            {
                "CatFactory: Scaffolding Made Easy",
                string.Empty,

                "How to use this code on your ASP.NET Core Application:",
                string.Empty,

                "1. Install EntityFrameworkCore.SqlServer package",
                string.Empty,

                "2. Register your DbContext and repositories in ConfigureServices method (Startup class):",
                string.Format("   services.AddDbContext<{0}>(options => options.UseSqlServer(\"ConnectionString\"));", project.GetDbContextName(project.Database)),

                "   services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "Happy scaffolding!",
                string.Empty,

                "You can check the guide for this package in:",
                "https://www.codeproject.com/Articles/1160615/Scaffolding-Entity-Framework-Core-with-CatFactory",
                string.Empty,
                "Also you can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.EntityFrameworkCore",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.EntityFrameworkCore.ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
