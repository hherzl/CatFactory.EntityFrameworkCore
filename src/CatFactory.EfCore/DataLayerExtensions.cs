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
        public static EfCoreProject GenerateDataLayer(this EfCoreProject project)
        {
            GenerateMappingDependencies(project);
            GenerateMappings(project);
            GenerateDbContext(project);
            GenerateDataContracts(project);
            GenerateDataRepositories(project);
            GenerateReadMe(project);

            return project;
        }

        private static void GenerateMappingDependencies(EfCoreProject project)
        {
            if (!project.Settings.UseDataAnnotations)
            {
                CSharpInterfaceBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerMappingDirectory(), project.Settings.ForceOverwrite, project.GetEntityMapperInterfaceDefinition(), project.GetEntityMapInterfaceDefinition());

                CSharpClassBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerMappingDirectory(), project.Settings.ForceOverwrite, project.GetEntityMapperClassDefinition(), project.GetDatabaseEntityMapperClassDefinition());
            }
        }

        private static void GenerateMappings(EfCoreProject project)
        {
            if (!project.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    CSharpClassBuilder
                        .CreateFiles(project.OutputDirectory, project.GetDataLayerMappingDirectory(), project.Settings.ForceOverwrite, table.GetEntityMapClassDefinition(project));
                }

                foreach (var view in project.Database.Views)
                {
                    CSharpClassBuilder
                        .CreateFiles(project.OutputDirectory, project.GetDataLayerMappingDirectory(), project.Settings.ForceOverwrite, view.GetEntityMapClassDefinition(project));
                }
            }
        }

        private static void GenerateDbContext(EfCoreProject project)
        {
            foreach (var projectFeature in project.Features)
            {
                CSharpClassBuilder
                    .CreateFiles(project.OutputDirectory, project.GetDataLayerDirectory(), project.Settings.ForceOverwrite, projectFeature.GetDbContextClassDefinition());
            }
        }

        private static void GenerateDataLayerContract(EfCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
            => CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.Settings.ForceOverwrite, interfaceDefinition);

        private static void GenerateRepositoryInterface(EfCoreProject project)
            => CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.Settings.ForceOverwrite, project.GetRepositoryInterfaceDefinition());

        private static void GenerateBaseRepositoryClassDefinition(EfCoreProject project)
            => CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.Settings.ForceOverwrite, project.GetRepositoryBaseClassDefinition());

        private static void GenerateRepositoryExtensionsClassDefinition(EfCoreProject project)
            => CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.Settings.ForceOverwrite, project.GetRepositoryExtensionsClassDefinition());

        private static void GenerateDataContracts(EfCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                if (!project.Settings.EntitiesWithDataContracts.Contains(table.FullName))
                {
                    continue;
                }

                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<string>()
                    {
                        "System"
                    },
                    Namespace = project.GetDataLayerDataContractsNamespace(),
                    Name = table.GetDataContractName()
                };

                var typeResolver = new ClrTypeResolver();

                foreach (var column in table.Columns)
                {
                    var propertyName = column.GetPropertyName();

                    classDefinition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), propertyName));
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    foreach (var column in foreignTable?.GetColumnsWithOutPrimaryKey())
                    {
                        var target = string.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                        if (classDefinition.Properties.Where(item => item.Name == column.GetPropertyName()).Count() == 0)
                        {
                            classDefinition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), target));
                        }
                    }
                }

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDataContractsDirectory(), project.Settings.ForceOverwrite, classDefinition);
            }
        }

        private static void GenerateDataRepositories(EfCoreProject project)
        {
            if (!string.IsNullOrEmpty(project.Settings.ConcurrencyToken))
            {
                project.UpdateExclusions.Add(project.Settings.ConcurrencyToken);
            }

            GenerateRepositoryInterface(project);
            GenerateBaseRepositoryClassDefinition(project);
            GenerateRepositoryExtensionsClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                var interfaceDef = repositoryClassDefinition.RefactInterface();

                interfaceDef.Implements.Add("IRepository");

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContract(project, interfaceDef);

                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.Settings.ForceOverwrite, repositoryClassDefinition);
            }
        }

        private static void GenerateReadMe(this EfCoreProject project)
        {
            var lines = new List<string>()
            {
                "CatFactory: Code Generation Made Easy",
                string.Empty,

                "How to use this code on your ASP.NET Core Application",
                string.Empty,

                "Register objects in Startup class, register your DbContext and repositories in ConfigureServices method:",
                " services.AddEntityFrameworkSqlServer().AddDbContext<StoreDbContext>();",
                " services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "Happy code generation!",
                string.Empty,

                "You can check the full guide to use this tool in:",
                "https://www.codeproject.com/Articles/1160615/Generating-Code-for-EF-Core-with-CatFactory",
                string.Empty,
                "Also you can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.EfCore",
                string.Empty,
                "*** Soon CatFactory will generate code for EF Core 2.0 (November - 2017) ***",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
