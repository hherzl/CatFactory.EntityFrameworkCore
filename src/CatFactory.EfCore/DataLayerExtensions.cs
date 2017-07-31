using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.DotNetCore;
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

            return project;
        }

        private static void GenerateAppSettings(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new AppSettingsClassDefinition
                {
                    Namespace = project.GetDataLayerNamespace()
                },
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
                        ObjectDefinition = new IEntityMapperInterfaceDefinition
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapperClassDefinition
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpInterfaceBuilder
                    {
                        ObjectDefinition = new IEntityMapInterfaceDefinition
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder()
                    {
                        ObjectDefinition = new DatabaseMapperClassDefinition(project)
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
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
                    ObjectDefinition = new DbContextClassDefinition(project, projectFeature)
                    {
                        Namespace = project.GetDataLayerNamespace()
                    },
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
                ObjectDefinition = new IRepositoryInterfaceDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateBaseRepositoryClassDefinition(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new RepositoryBaseClassDefinition(project)
                {
                    Namespace = project.GetDataLayerContractsNamespace()
                },
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

                var resolver = new ClrTypeResolver() as ITypeResolver;

                var classDefinition = new CSharpClassDefinition
                {
                    Namespaces = new List<String>() { "System" },
                    Namespace = project.GetDataLayerDataContractsNamespace(),
                    Name = table.GetDataContractName()
                };

                foreach (var column in table.Columns)
                {
                    var propertyName = column.GetPropertyName();

                    classDefinition.Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), propertyName));
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
                            classDefinition.Properties.Add(new PropertyDefinition(resolver.Resolve(column.Type), target));
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
    }
}
