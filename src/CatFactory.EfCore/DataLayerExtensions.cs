using System.Collections.Generic;
using CatFactory.DotNetCore;

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
            if (!project.UseDataAnnotations)
            {
                var codeBuilders = new List<DotNetCodeBuilder>()
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
                        ObjectDefinition = new DbMapperClassDefinition(project.Database)
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
            if (!project.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    var codeBuilder = new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapClassDefinition(table, project)
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    };

                    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }

                foreach (var view in project.Database.Views)
                {
                    var codeBuilder = new CSharpClassBuilder
                    {
                        ObjectDefinition = new EntityMapClassDefinition(view, project)
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    };

                    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

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

                if (project.UseDataAnnotations)
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

        private static void GenerateDataLayerContracts(EfCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = interfaceDefinition,
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateRepositoryInterface(EfCoreProject project)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = new IRepositoryInterfaceDefinition
                {
                    Namespace = project.GetDataLayerContractsNamespace()
                },
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateBaseRepositoryClassDefinition(EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new BaseRepositoryClassDefinition(project)
                {
                    Namespace = project.GetDataLayerContractsNamespace()
                },
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
        }
        
        private static void GenerateDataRepositories(EfCoreProject project)
        {
            GenerateRepositoryInterface(project);
            GenerateBaseRepositoryClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = new RepositoryClassDefinition(project, projectFeature)
                    {
                        Namespace = project.GetDataLayerRepositoriesNamespace(),
                        Project = project
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());
                codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerContractsNamespace());

                var interfaceDef = (codeBuilder.ObjectDefinition as CSharpClassDefinition).RefactInterface();

                interfaceDef.Implements.Add("IRepository");

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContracts(project, interfaceDef);

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }

        }
    }
}
