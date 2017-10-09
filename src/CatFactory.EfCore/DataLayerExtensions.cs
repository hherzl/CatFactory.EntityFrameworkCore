using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            GenerateAppSettings(project);
            GenerateMappingDependencies(project);
            GenerateMappings(project);
            GenerateDbContext(project);
            GenerateDataContracts(project);
            GenerateDataRepositories(project);
            GenerateReadMe(project);

            return project;
        }

        private static void GenerateAppSettings(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new AppSettingsClassDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerDirectory());
        }

        private static void GenerateMappingDependencies(EfCoreProject project)
        {
            if (!project.Settings.UseDataAnnotations)
            {
                var codeBuilders = new List<CSharpCodeBuilder>()
                {
                    new CSharpInterfaceBuilder
                    {
                        ObjectDefinition = new EntityMapperInterfaceDefinition(project),
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapperClassDefinition(project),
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpInterfaceBuilder
                    {
                        ObjectDefinition = new EntityMapInterfaceDefinition(project),
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder()
                    {
                        ObjectDefinition = new DatabaseMapperClassDefinition(project),
                        OutputDirectory = project.OutputDirectory
                    },
                };

                foreach (var codeBuilder in codeBuilders)
                {
                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }
            }
        }

        private static void GenerateMappings(EfCoreProject project)
        {
            if (!project.Settings.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    var codeBuilder = new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapClassDefinition(table, project),
                        OutputDirectory = project.OutputDirectory
                    };

                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }

                foreach (var view in project.Database.Views)
                {
                    var codeBuilder = new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapClassDefinition(view, project),
                        OutputDirectory = project.OutputDirectory
                    };

                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }
            }
        }

        private static void GenerateDbContext(EfCoreProject project)
        {
            foreach (var projectFeature in project.Features)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = new DbContextClassDefinition(projectFeature),
                    OutputDirectory = project.OutputDirectory
                };

                if (project.Settings.UseDataAnnotations)
                {
                    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());
                }
                else
                {
                    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerMappingNamespace());
                }

                codeBuilder.CreateFile(project.GetDataLayerDirectory());
            }
        }

        private static void GenerateDataLayerContract(EfCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = interfaceDefinition,
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateRepositoryTest(EfCoreProject project, CSharpClassDefinition classDefinition)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = classDefinition,
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
        }

        private static void GenerateRepositoryInterface(EfCoreProject project)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = new RepositoryInterfaceDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateBaseRepositoryClassDefinition(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new RepositoryBaseClassDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
        }

        private static void GenerateRepositoryExtensionsClassDefinition(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new RepositoryExtensionsClassDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
        }

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
                    Namespaces = new List<String>() { "System" },
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

                    foreach (var column in foreignTable?.GetColumnsWithOutKey())
                    {
                        var target = String.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                        if (classDefinition.Properties.Where(item => item.Name == column.GetPropertyName()).Count() == 0)
                        {
                            classDefinition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), target));
                        }
                    }
                }

                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = classDefinition,
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetDataLayerDataContractsDirectory());
            }
        }

        private static void GenerateDataRepositories(EfCoreProject project)
        {
            if (!String.IsNullOrEmpty(project.Settings.ConcurrencyToken))
            {
                project.UpdateExclusions.Add(project.Settings.ConcurrencyToken);
            }

            GenerateRepositoryInterface(project);
            GenerateBaseRepositoryClassDefinition(project);
            GenerateRepositoryExtensionsClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = new RepositoryClassDefinition(projectFeature);

                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = repositoryClassDefinition,
                    OutputDirectory = project.OutputDirectory
                };

                var interfaceDef = repositoryClassDefinition.RefactInterface();

                interfaceDef.Implements.Add("IRepository");

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContract(project, interfaceDef);

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }
        }

        private static void GenerateReadMe(this EfCoreProject project)
        {
            var lines = new List<String>();

            lines.Add("CatFactory: Code Generation Made Easy");
            lines.Add(String.Empty);

            lines.Add("How to use this code on your ASP.NET Core Application");
            lines.Add(String.Empty);

            lines.Add("Register objects in Startup class, register your DbContext and repositories in ConfigureServices method:");
            lines.Add(" services.AddEntityFrameworkSqlServer().AddDbContext<StoreDbContext>();");
            lines.Add(" services.AddScoped<IDboRepository, DboRepository>();");
            lines.Add(String.Empty);

            lines.Add("Happy code generation!");
            lines.Add(String.Empty);

            lines.Add("You can check the full guide to use this tool in:");
            lines.Add("https://www.codeproject.com/Articles/1160615/Generating-Code-for-EF-Core-with-CatFactory");
            lines.Add(String.Empty);
            lines.Add("Also you can check source code on GitHub:");
            lines.Add("https://github.com/hherzl/CatFactory.EfCore");
            lines.Add(String.Empty);
            lines.Add("*** Soon CatFactory will generate code for EF Core 2.0 (November - 2017) ***");
            lines.Add(String.Empty);
            lines.Add("CatFactory Development Team ==^^==");

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
