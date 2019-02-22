using System.Collections.Generic;
using System.IO;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

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
            project.ObjectDefinitions = new List<IObjectDefinition>();

            var projectSelection = project.GlobalSelection();

            if (!projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    project.ObjectDefinitions.Add(project.GetEntityConfigurationClassDefinition(table));

                    //var codeBuilder = new CSharpClassBuilder
                    //{
                    //    OutputDirectory = project.OutputDirectory,
                    //    ForceOverwrite = projectSelection.Settings.ForceOverwrite,
                    //    ObjectDefinition = project.GetEntityConfigurationClassDefinition(table)
                    //};

                    //codeBuilder.CreateFile(project.GetDataLayerConfigurationsDirectory(project.Database.HasDefaultSchema(table) ? "" : table.Schema));

                    //if (project.Database.HasDefaultSchema(table))
                    //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityConfigurationClassDefinition(table));
                    //else
                    //    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(table.Schema), projectSelection.Settings.ForceOverwrite, project.GetEntityConfigurationClassDefinition(table));
                }

                foreach (var view in project.Database.Views)
                {
                    project.ObjectDefinitions.Add(project.GetEntityConfigurationClassDefinition(view));
                }

                project.Scaffold();

                //foreach (var view in project.Database.Views)
                //{
                //    if (project.Database.HasDefaultSchema(view))
                //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityTypeConfigurationClassDefinition(view));
                //    else
                //        CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(view.Schema), projectSelection.Settings.ForceOverwrite, project.GetEntityTypeConfigurationClassDefinition(view));
                //}
            }
        }

        private static void ScaffoldDbContext(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            //foreach (var projectFeature in project.Features)
            //{
            //    CSharpCodeBuilder
            //        .CreateFiles(project.OutputDirectory, project.GetDataLayerDirectory(), projectSelection.Settings.ForceOverwrite, projectFeature.GetDbContextClassDefinition(projectSelection));
            //}

            project.ObjectDefinitions = new List<IObjectDefinition>
            {
                project.GetDbContextClassDefinition(projectSelection)
            };

            project.Scaffold();
        }

        private static void ScaffoldRepositoryInterface(EntityFrameworkCoreProject project)
        {
            project.ObjectDefinitions = new List<IObjectDefinition>
            {
                project.GetRepositoryInterfaceDefinition()
            };

            project.Scaffold();

            //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryInterfaceDefinition());
        }

        private static void ScaffoldBaseRepositoryClassDefinition(EntityFrameworkCoreProject project)
        {
            project.ObjectDefinitions = new List<IObjectDefinition>
            {
                project.GetRepositoryBaseClassDefinition()
            };

            project.Scaffold();

            //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryBaseClassDefinition());
        }

        private static void ScaffoldRepositoryExtensionsClassDefinition(EntityFrameworkCoreProject project)
        {
            project.ObjectDefinitions = new List<IObjectDefinition>
            {
                project.GetRepositoryExtensionsClassDefinition()
            };

            project.Scaffold();

            //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryExtensionsClassDefinition());
        }

        //private static void ScaffoldDataLayerContract(EntityFrameworkCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
        //{
        //    project.ObjectDefinitions = new List<IObjectDefinition>
        //    {
        //        interfaceDefinition
        //    };

        //    project.Scaffold();

        //    //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.GlobalSelection().Settings.ForceOverwrite, interfaceDefinition);
        //}
        
        private static void ScaffoldDataContracts(EntityFrameworkCoreProject project)
        {
            project.ObjectDefinitions = new List<IObjectDefinition>();

            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                if (!selection.Settings.EntitiesWithDataContracts)
                    continue;

                project.ObjectDefinitions.Add(project.GetDataContractClassDefinition(table));

                //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDataContractsDirectory(), selection.Settings.ForceOverwrite, classDefinition);
            }


            //foreach (var views in project.Database.Views)
            //{
            //    var selection = project.GetSelection(views);

            //    if (!selection.Settings.EntitiesWithDataContracts)
            //        continue;

            //    project.ObjectDefinitions.Add(project.GetDataContractClassDefinition(views));

            //    //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDataContractsDirectory(), selection.Settings.ForceOverwrite, classDefinition);
            //}

            project.Scaffold();
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

                project.ObjectDefinitions = new List<IObjectDefinition>
            {
                repositoryClassDefinition
            };

                project.Scaffold();

                var repositoryInterfaceDefinition = repositoryClassDefinition.RefactInterface();

                repositoryInterfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();
                repositoryInterfaceDefinition.Implements.Add("IRepository");




                project.ObjectDefinitions = new List<IObjectDefinition>
            {
                repositoryInterfaceDefinition
            };

                project.Scaffold();

                //ScaffoldDataLayerContract(project, repositoryInterfaceDefinition);

                //CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), projectSelection.Settings.ForceOverwrite, repositoryClassDefinition);
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
