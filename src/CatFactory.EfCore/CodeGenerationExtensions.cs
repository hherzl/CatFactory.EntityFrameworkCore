using System;
using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class CodeGenerationExtensions
    {
        public static EfCoreProject GenerateEntities(this EfCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var codeBuilder = new CSharpClassBuilder()
                {
                    ObjectDefinition = new EntityClassDefinition(table)
                    {
                        Namespace = project.GetEntityLayerNamespace(),
                    },
                    OutputDirectory = project.OutputDirectory
                };

                if (project.UseDataAnnotations)
                {
                    codeBuilder.ObjectDefinition.Namespaces.Add("System.ComponentModel.DataAnnotations");
                    codeBuilder.ObjectDefinition.Namespaces.Add("System.ComponentModel.DataAnnotations.Schema");

                    codeBuilder.ObjectDefinition.Attributes.Add(new MetadataAttribute("Table")
                    {
                        Arguments = new List<String>
                        {
                            String.Format("\"{0}\"", table.Name),
                            String.Format("Schema = \"{0}\"", table.Schema),
                        }
                    });

                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        var column = table.Columns[i];

                        foreach (var property in codeBuilder.ObjectDefinition.Properties)
                        {
                            if (column.GetPropertyName() == property.Name)
                            {
                                if (table.Identity != null && table.Identity.Name == column.Name)
                                {
                                    property.Attributes.Add(new MetadataAttribute("DatabaseGenerated")
                                    {
                                        Arguments = new List<String>() { "DatabaseGeneratedOption.Identity" }
                                    });
                                }

                                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                                {
                                    property.Attributes.Add(new MetadataAttribute("Key"));
                                }

                                property.Attributes.Add(new MetadataAttribute("Column")
                                {
                                    Arguments = new List<String> { String.Format("Order = {0}", i + 1) }
                                });

                                if (!column.Nullable)
                                {
                                    property.Attributes.Add(new MetadataAttribute("Required"));
                                }

                                if (column.Type.Contains("char") && column.Length > 0)
                                {
                                    property.Attributes.Add(new MetadataAttribute("StringLength")
                                    {
                                        Arguments = new List<String>() { column.Length.ToString() }
                                    });
                                }
                            }
                        }
                    }
                }

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            foreach (var view in project.Database.Views)
            {
                var codeBuilder = new CSharpClassBuilder()
                {
                    ObjectDefinition = new EntityClassDefinition(view)
                    {
                        Namespace = project.GetEntityLayerNamespace()
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            return project;
        }

        public static EfCoreProject GenerateAppSettings(this EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder()
            {
                ObjectDefinition = new AppSettingsClassDefinition()
                {
                    Namespace = project.GetDataLayerNamespace()
                },
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerDirectory());

            return project;
        }

        public static EfCoreProject GenerateMappingDependences(this EfCoreProject project)
        {
            if (!project.UseDataAnnotations)
            {
                var codeBuilders = new List<DotNetCodeBuilder>()
                {
                    new CSharpInterfaceBuilder()
                    {
                        ObjectDefinition = new IEntityMapperInterfaceDefinition() { Namespace = project.GetDataLayerMappingNamespace() },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder()
                    {
                        ObjectDefinition = new EntityMapperClassDefinition() { Namespace = project.GetDataLayerMappingNamespace() },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpInterfaceBuilder()
                    {
                        ObjectDefinition = new IEntityMapInterfaceDefinition() { Namespace = project.GetDataLayerMappingNamespace() },
                        OutputDirectory = project.OutputDirectory
                    },
                    new CSharpClassBuilder()
                    {
                        ObjectDefinition = new DbMapperClassDefinition(project.Database) { Namespace = project.GetDataLayerMappingNamespace() },
                        OutputDirectory = project.OutputDirectory
                    },
                };

                foreach (var codeBuilder in codeBuilders)
                {
                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }
            }

            return project;
        }

        public static EfCoreProject GenerateMappings(this EfCoreProject project)
        {
            if (!project.UseDataAnnotations)
            {
                foreach (var table in project.Database.Tables)
                {
                    var codeBuilder = new CSharpClassBuilder()
                    {
                        ObjectDefinition = new MappingClassDefinition(table)
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
                    var codeBuilder = new CSharpClassBuilder()
                    {
                        ObjectDefinition = new MappingClassDefinition(view)
                        {
                            Namespace = project.GetDataLayerMappingNamespace()
                        },
                        OutputDirectory = project.OutputDirectory
                    };

                    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

                    codeBuilder.CreateFile(project.GetDataLayerMappingDirectory());
                }
            }

            return project;
        }

        public static EfCoreProject GenerateDbContext(this EfCoreProject project)
        {
            foreach (var projectFeature in project.Features)
            {
                var codeBuilder = new CSharpClassBuilder()
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

            return project;
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

        public static EfCoreProject GenerateDataRepositories(this EfCoreProject project)
        {
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

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContracts(project, interfaceDef);

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }

            return project;
        }

        public static EfCoreProject GenerateViewModels(this EfCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var codeBuilder = new CSharpClassBuilder()
                {
                    ObjectDefinition = new ViewModelClassDefinition(table)
                    {
                        Namespace = project.GetDataLayerDataContractsNamespace(),
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetDataLayerDataContractsDirectory());
            }

            return project;
        }

        private static void GenerateBusinessLayerContracts(EfCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = interfaceDefinition,
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

            codeBuilder.CreateFile(project.GetBusinessLayerContractsDirectory());
        }

        public static EfCoreProject GenerateBusinessObject(this EfCoreProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new CSharpClassDefinition()
                {
                    Name = "BusinessObject",
                    Namespace = project.GetBusinessLayerNamespace()
                },
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerNamespace());

            codeBuilder.CreateFile(project.GetBusinessLayerDirectory());

            return project;
        }

        public static EfCoreProject GenerateBusinessObjects(this EfCoreProject project)
        {
            project.GenerateBusinessObject();

            foreach (var projectFeature in project.Features)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = new BusinessObjectClassDefinition(projectFeature)
                    {
                        Namespace = project.GetBusinessLayerNamespace()
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerNamespace());
                codeBuilder.ObjectDefinition.Namespaces.Add(project.GetBusinessLayerContractsNamespace());

                var interfaceDef = (codeBuilder.ObjectDefinition as CSharpClassDefinition).RefactInterface();

                interfaceDef.Namespace = project.GetBusinessLayerContractsNamespace();

                GenerateBusinessLayerContracts(project, interfaceDef);

                codeBuilder.CreateFile(project.GetBusinessLayerDirectory());
            }

            return project;
        }

        public static EfCoreProject GenerateBusinessInterfacesResponses(this EfCoreProject project)
        {
            var interfacesDefinitions = new List<CSharpInterfaceDefinition>()
            {
                new ResponseInterfaceDefinition(),
                new SingleModelResponseInterfaceDefinition(),
                new ListModelResponseInterfaceDefinition()
            };

            foreach (var definition in interfacesDefinitions)
            {
                definition.Namespace = project.GetBusinessLayerResponsesNamespace();

                var codeBuilder = new CSharpInterfaceBuilder
                {
                    ObjectDefinition = definition,
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetBusinessLayerResponsesDirectory());
            }

            return project;
        }

        public static EfCoreProject GenerateBusinessClassesResponses(this EfCoreProject project)
        {
            var classesDefinitions = new List<CSharpClassDefinition>()
            {
                new SingleModelResponseClassDefinition(),
                new ListModelResponseClassDefinition()
            };

            foreach (var definition in classesDefinitions)
            {
                definition.Namespace = project.GetBusinessLayerResponsesNamespace();

                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = definition,
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.CreateFile(project.GetBusinessLayerResponsesDirectory());
            }

            return project;
        }
    }
}
