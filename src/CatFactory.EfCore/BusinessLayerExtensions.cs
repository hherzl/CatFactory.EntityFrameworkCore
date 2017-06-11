using System.Collections.Generic;
using CatFactory.DotNetCore;

namespace CatFactory.EfCore
{
    public static class BusinessLayerExtensions
    {
        public static EfCoreProject GenerateViewModels(this EfCoreProject project)
        {
            foreach (var table in project.Database.Tables)
            {
                var codeBuilder = new CSharpClassBuilder()
                {
                    ObjectDefinition = new ViewModelClassDefinition(project, table)
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
