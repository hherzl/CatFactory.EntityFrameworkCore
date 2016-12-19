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
                                        Arguments = new List<String>()
                                        {
                                            "DatabaseGeneratedOption.Identity"
                                        }
                                    });
                                }

                                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                                {
                                    property.Attributes.Add(new MetadataAttribute("Key"));
                                }

                                property.Attributes.Add(new MetadataAttribute("Column")
                                {
                                    Arguments = new List<string>
                                    {
                                        String.Format("Order = {0}", i + 1)
                                    }
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

        public static EfCoreProject GenerateContracts(this EfCoreProject project)
        {
            foreach (var projectFeature in project.Features)
            {
                var codeBuilder = new CSharpInterfaceBuilder
                {
                    ObjectDefinition = new RepositoryInterfaceDefinition(projectFeature)
                    {
                        Namespace = project.GetDataLayerContractsNamespace()
                    },
                    OutputDirectory = project.OutputDirectory
                };

                codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

                codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
            }

            return project;
        }

        public static EfCoreProject GenerateRepositories(this EfCoreProject project)
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

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }

            return project;
        }
    }
}
