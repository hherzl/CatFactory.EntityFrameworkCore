using System;
using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class RepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public RepositoryInterfaceDefinition(ProjectFeature projectFeature)
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");

            Name = projectFeature.GetInterfaceRepositoryName();

            foreach (var dbObject in projectFeature.DbObjects)
            {
                Methods.Add(new MethodDefinition(String.Format("IEnumerable<{0}>", dbObject.GetEntityName()), String.Format("Get{0}", dbObject.GetPluralName())));

                Methods.Add(new MethodDefinition(dbObject.GetEntityName(), String.Format("Get{0}", dbObject.GetEntityName()))
                {
                    Parameters = new List<ParameterDefinition>()
                    {
                        new ParameterDefinition(dbObject.GetEntityName(), "entity")
                    }
                });

                if (!projectFeature.IsView(dbObject))
                {
                    Methods.Add(new MethodDefinition("void", String.Format("Add{0}", dbObject.GetEntityName()))
                    {
                        Parameters = new List<ParameterDefinition>()
                        {
                            new ParameterDefinition(dbObject.GetEntityName(), "entity")
                        }
                    });

                    Methods.Add(new MethodDefinition("void", String.Format("Update{0}", dbObject.GetEntityName()))
                    {
                        Parameters = new List<ParameterDefinition>()
                        {
                            new ParameterDefinition(dbObject.GetEntityName(), "changes")
                        }
                    });

                    Methods.Add(new MethodDefinition("void", String.Format("Delete{0}", dbObject.GetEntityName()))
                    {
                        Parameters = new List<ParameterDefinition>()
                        {
                            new ParameterDefinition(dbObject.GetEntityName(), "entity")
                        }
                    });
                }
            }
        }
    }
}
