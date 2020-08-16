using System.IO;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.Markdown;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DomainExtensions
    {
        public static EntityFrameworkCoreProject ScaffoldDomain(this EntityFrameworkCoreProject project)
        {
            ScaffoldEntityInterface(project);
            ScaffoldModels(project);

            ScaffoldConfigurations(project);

            ScaffoldDbContext(project);

            ScaffoldQueryModels(project);

            ScaffoldExtensions(project);

            ScaffoldMdReadMe(project);

            return project;
        }

        internal static void ScaffoldEntityInterface(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetEntityInterfaceDefinition(true), project.GetDomainModelsDirectory());

            if (project.GlobalSelection().Settings.AuditEntity != null)
                project.Scaffold(project.GetAuditEntityInterfaceDefinition(true), project.GetDomainModelsDirectory());
        }

        internal static EntityFrameworkCoreProject ScaffoldModels(this EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                var definition = project.GetEntityClassDefinition(table, true);

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(table, project);

                project.Scaffold(definition, project.GetDomainModelsDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var definition = project.GetEntityClassDefinition(view, project.Database.HasDefaultSchema(view) ? project.GetDomainModelsNamespace() : project.GetDomainModelsNamespace(view.Schema));

                if (selection.Settings.UseDataAnnotations)
                    definition.AddDataAnnotations(view, project);

                project.Scaffold(definition, project.GetDomainModelsDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
            }

            return project;
        }

        internal static void ScaffoldConfigurations(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    var definition = project.GetEntityConfigurationClassDefinition(table, true);

                    project.Scaffold(definition, project.GetDomainConfigurationsDirectory(), project.Database.HasDefaultSchema(table) ? "" : table.Schema);
                }

                foreach (var view in project.Database.Views)
                {
                    var definition = project.GetEntityConfigurationClassDefinition(view, true);

                    project.Scaffold(definition, project.GetDomainConfigurationsDirectory(), project.Database.HasDefaultSchema(view) ? "" : view.Schema);
                }
            }
        }

        internal static void ScaffoldDbContext(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            project.Scaffold(project.GetDbContextClassDefinition(projectSelection, true), project.GetDomainDirectory());
        }

        internal static void ScaffoldQueryModels(EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                if (!selection.Settings.EntitiesWithDataContracts)
                    continue;

                project.Scaffold(project.GetQueryModelClassDefinition(table), project.GetDomainQueryModelsDirectory());
            }
        }

        internal static void ScaffoldExtensions(EntityFrameworkCoreProject project)
        {
            project.Scaffold(project.GetPagingExtensionsClassDefinition(true), project.GetDomainDirectory());

            foreach (var projectFeature in project.Features)
            {
                project.Scaffold(projectFeature.GetDbContextQueryExtensionsClassDefinition(), project.GetDomainDirectory());
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

            File.WriteAllText(Path.Combine(project.OutputDirectory, "README.md"), readMe.ToString());
        }
    }
}
