using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.EfCore.Definitions;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class DataLayerExtensions
    {
        public static EntityFrameworkCoreProject ScaffoldDataLayer(this EntityFrameworkCoreProject project)
        {
            ScaffoldMappingDependencies(project);
            ScaffoldMappings(project);
            ScaffoldDbContext(project);
            ScaffoldDataContracts(project);
            ScaffoldDataRepositories(project);
            ScaffoldReadMe(project);

            return project;
        }

        private static void ScaffoldMappingDependencies(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!projectSelection.Settings.UseDataAnnotations)
            {
                CSharpCodeBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityMapperInterfaceDefinition(), project.GetEntityTypeConfigurationInterfaceDefinition());

                CSharpCodeBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityMapperClassDefinition(), project.GetDatabaseEntityMapperClassDefinition());
            }
        }

        private static void ScaffoldMappings(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!projectSelection.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    CSharpCodeBuilder
                        .CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityTypeConfigurationClassDefinition(table));
                }

                foreach (var view in project.Database.Views)
                {
                    CSharpCodeBuilder
                        .CreateFiles(project.OutputDirectory, project.GetDataLayerConfigurationsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetEntityTypeConfigurationClassDefinition(view));
                }
            }
        }

        private static void ScaffoldDbContext(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            foreach (var projectFeature in project.Features)
            {
                CSharpCodeBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerDirectory(), projectSelection.Settings.ForceOverwrite, projectFeature.GetDbContextClassDefinition(projectSelection));
            }
        }

        private static void ScaffoldDataLayerContract(EntityFrameworkCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
            => CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.GlobalSelection().Settings.ForceOverwrite, interfaceDefinition);

        private static void ScaffoldRepositoryInterface(EntityFrameworkCoreProject project)
            => CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryInterfaceDefinition());

        private static void ScaffoldBaseRepositoryClassDefinition(EntityFrameworkCoreProject project)
            => CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryBaseClassDefinition());

        private static void ScaffoldRepositoryExtensionsClassDefinition(EntityFrameworkCoreProject project)
            => CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetRepositoryExtensionsClassDefinition());

        private static void ScaffoldDataContracts(EntityFrameworkCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                if (!selection.Settings.EntitiesWithDataContracts)
                {
                    continue;
                }

                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<string>
                    {
                        "System"
                    },
                    Namespace = project.GetDataLayerDataContractsNamespace(),
                    Name = table.GetDataContractName()
                };

                foreach (var column in table.Columns)
                {
                    classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), column.GetPropertyName()));
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                    {
                        var target = string.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                        if (classDefinition.Properties.Where(item => item.Name == column.GetPropertyName()).Count() == 0)
                        {
                            classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), target));
                        }
                    }
                }

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDataContractsDirectory(), selection.Settings.ForceOverwrite, classDefinition);
            }
        }

        private static void ScaffoldDataRepositories(EntityFrameworkCoreProject project)
        {
            var projectSelection = project.GlobalSelection();

            if (!string.IsNullOrEmpty(projectSelection.Settings.ConcurrencyToken))
            {
                project.UpdateExclusions.Add(projectSelection.Settings.ConcurrencyToken);
            }

            ScaffoldRepositoryInterface(project);
            ScaffoldBaseRepositoryClassDefinition(project);
            ScaffoldRepositoryExtensionsClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                var interfaceDef = repositoryClassDefinition.RefactInterface();

                interfaceDef.Implements.Add("IRepository");

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                ScaffoldDataLayerContract(project, interfaceDef);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), projectSelection.Settings.ForceOverwrite, repositoryClassDefinition);
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

                "1. Install packages for EntityFrameworkCore and EntityFrameworkCore.SqlServer",
                string.Empty,

                "2. Register your DbContext and repositories in ConfigureServices method (Startup class):",
                string.Format(" services.AddDbContext<{0}>(options => options.UseSqlServer(Configuration[\"ConnectionString\"]));", project.Database.GetDbContextName()),

                " services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "Happy scaffolding!",
                string.Empty,

                "You can check the guide for this package in:",
                "https://www.codeproject.com/Articles/1160615/Scaffolding-Entity-Framework-Core-with-CatFactory",
                string.Empty,
                "Also you can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.EntityFrameworkCore",
                string.Empty,
                "*** Soon CatFactory will scaffold code for Entity Framework Core 2.0 (February - 2018) ***",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "CatFactory.EfCore.ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
