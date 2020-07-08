using System.IO;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.Markdown;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DataLayerExtensions
    {
        public static EntityFrameworkCoreProject ScaffoldDataLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldConfigurations(project);
            ScaffoldDbContextInternal(project);
            ScaffoldDataContracts(project);
            ScaffoldDataRepositoriesInternal(project);
            ScaffoldMdReadMe(project);

            return project;
        }

        public static EntityFrameworkCoreProject ScaffoldDbContext(this EntityFrameworkCoreProject project)
        {
            ScaffoldConfigurations(project);
            ScaffoldDbContextInternal(project);

            return project;
        }

        public static EntityFrameworkCoreProject ScaffoldDataRepositories(this EntityFrameworkCoreProject project)
        {
            ScaffoldDataContracts(project);
            ScaffoldDataRepositoriesInternal(project);

            return project;
        }

        internal static void ScaffoldConfigurations(EntityFrameworkCoreProject project)
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

        internal static void ScaffoldDbContextInternal(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            project.Scaffold(project.GetDbContextClassDefinition(projectSelection), project.GetDataLayerDirectory());
        }

        internal static void ScaffoldRepositoryInterface(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryInterfaceDefinition(), project.GetDataLayerContractsDirectory());
        }

        internal static void ScaffoldBaseRepositoryClassDefinition(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryBaseClassDefinition(), project.GetDataLayerRepositoriesDirectory());
        }

        internal static void ScaffoldRepositoryExtensionsClassDefinition(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetRepositoryExtensionsClassDefinition(), project.GetDataLayerRepositoriesDirectory());
        }

        internal static void ScaffoldDataContracts(EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                if (!selection.Settings.EntitiesWithDataContracts)
                    continue;

                project.Scaffold(project.GetDataContractClassDefinition(table), project.GetDataLayerDataContractsDirectory());
            }
        }

        internal static void ScaffoldDataRepositoriesInternal(EntityFrameworkCoreProject project)
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

        internal static void ScaffoldMdReadMe(this EntityFrameworkCoreProject project)
        {
            var readMe = new MdDocument();

            readMe.H1("CatFactory ==^^==: Scaffolding Made Easy");

            readMe.WriteLine("How to use this code on your ASP.NET Core Application:");

            readMe.OrderedList(
                "Install EntityFrameworkCore.SqlServer package",
                "Register the DbContext and Repositories in ConfigureServices method (Startup class)"
                );

            readMe.H2("Install package");

            readMe.WriteLine("You can install the NuGet packages in Visual Studio or Windows Command Line, for more info:");

            readMe.WriteLine(
                Md.Link("Install and manage packages with the Package Manager Console in Visual Studio (PowerShell)", "https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell")
                );

            readMe.WriteLine(
                Md.Link(".NET Core CLI", "https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package")
                );

            readMe.H2("Register DbContext and Repositories");

            readMe.WriteLine("Add the following code lines in {0} method (Startup class):", Md.Bold("ConfigureServices"));
            readMe.WriteLine("  services.AddDbContext<{0}>(options => options.UseSqlServer(\"ConnectionString\"));", project.GetDbContextName(project.Database));
            readMe.WriteLine("  services.AddScope<{0}, {1}>()", "IDboRepository", "DboRepository");

            readMe.WriteLine("Happy scaffolding!");

            var codeProjectLink = Md.Link("Scaffolding Entity Framework Core with CatFactory", "https://www.codeproject.com/Articles/1160615/Scaffolding-Entity-Framework-Core-with-CatFactory");

            readMe.WriteLine("You can check the guide for this package in: {0}", codeProjectLink);

            var gitHubRepositoryLink = Md.Link("GitHub repository", "https://github.com/hherzl/CatFactory.EntityFrameworkCore");

            readMe.WriteLine("Also you can check the source code on {0}", gitHubRepositoryLink);

            readMe.WriteLine("CatFactory Development Team ==^^==");

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.EntityFrameworkCore.ReadMe.MD"), readMe.ToString());
        }
    }
}
