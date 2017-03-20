using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class RepositoryClassDefinition : CSharpClassDefinition
    {
        public RepositoryClassDefinition(EfCoreProject project, ProjectFeature projectFeature)
        {
            Project = project;

            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = projectFeature.GetClassRepositoryName();

            BaseClass = "Repository";

            Implements.Add(projectFeature.GetInterfaceRepositoryName());

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(projectFeature.Database.GetDbContextName(), "dbContext"))
            {
                ParentInvoke = "base(dbContext)"
            });

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = projectFeature.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                if (project.Settings.EntitiesWithDataContracts.Contains(table.FullName) && !Namespaces.Contains(project.GetDataLayerDataContractsNamespace()))
                {
                    Namespaces.Add(project.GetDataLayerDataContractsNamespace());
                }

                Methods.Add(GetGetAllMethod(projectFeature, table));

                AddGetByUniqueMethods(projectFeature, table);

                Methods.Add(GetGetMethod(projectFeature, table));
                Methods.Add(GetAddMethod(projectFeature, table));
                Methods.Add(GetUpdateMethod(projectFeature, table));
                Methods.Add(GetDeleteMethod(projectFeature, table));
            }
        }

        public EfCoreProject Project { get; private set; }

        public MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var returnType = String.Empty;

            var lines = new List<ILine>();

            var tableCast = dbObject as Table;

            if (tableCast == null)
            {
                returnType = dbObject.GetSingularName();

                lines.Add(new CodeLine("return Paging<{0}>(pageSize, pageNumber);", dbObject.GetSingularName()));
            }
            else
            {
                if (Project.Settings.EntitiesWithDataContracts.Contains(tableCast.FullName))
                {
                    // todo: add logic to generate data contract

                    var entityAlias = CatFactory.NamingConvention.GetCamelCase(tableCast.GetEntityName());

                    returnType = tableCast.GetDataContractName();

                    var dataContractPropertiesSets = new[] { new { Source = String.Empty, Target = String.Empty } }.ToList();

                    foreach (var column in tableCast.Columns)
                    {
                        var propertyName = column.GetPropertyName();

                        dataContractPropertiesSets.Add(new { Source = String.Format("{0}.{1}", entityAlias, propertyName), Target = propertyName });
                    }

                    foreach (var foreignKey in tableCast.ForeignKeys)
                    {
                        var foreignTable = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == foreignKey.References);

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        foreach (var column in foreignTable?.GetColumnsWithOutKey())
                        {
                            if (dataContractPropertiesSets.Where(item => item.Source == String.Format("{0}.{1}", entityAlias, column.GetPropertyName())).Count() == 0)
                            {
                                var source = String.Format("{0}.{1}", foreignKeyAlias, column.GetPropertyName());
                                var target = String.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                                dataContractPropertiesSets.Add(new { Source = source, Target = target });
                            }
                        }
                    }

                    lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, tableCast.GetEntityName()));

                    foreach (var foreignKey in tableCast.ForeignKeys)
                    {
                        var foreignTable = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == foreignKey.References);

                        var foreignKeyEntityName = foreignTable.GetEntityName();

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        if (foreignKey.Key.Count == 0)
                        {
                            lines.Add(new CommentLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                        }
                        else if (foreignKey.Key.Count == 1)
                        {
                            if (foreignTable == null)
                            {
                                lines.Add(new CommentLine(1, " There isn't definition for '{0}' in your current database", foreignKey.References));
                            }
                            else
                            {
                                var column = tableCast.Columns.FirstOrDefault(item => item.Name == foreignKey.Key[0]);

                                var x = NamingConvention.GetPropertyName(foreignKey.Key[0]);
                                var y = NamingConvention.GetPropertyName(foreignTable.PrimaryKey.Key[0]);

                                if (column.Nullable)
                                {
                                    lines.Add(new CodeLine(1, "join {0}Join in DbContext.Set<{1}>() on {2}.{3} equals {0}Join.{4} into {0}Temp", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                    lines.Add(new CodeLine(2, "from {0} in {0}Temp.Where(relation => relation.{2} == {1}.{3}).DefaultIfEmpty()", foreignKeyAlias, entityAlias, x, y));
                                }
                                else
                                {
                                    lines.Add(new CodeLine(1, "join {0} in DbContext.Set<{1}>() on {2}.{3} equals {0}.{4}", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                }
                            }
                        }
                        else
                        {
                            // todo: add logic for foreignkey with multiple key
                        }
                    }

                    lines.Add(new CodeLine(1, "select new {0}", returnType));
                    lines.Add(new CodeLine(1, "{{"));

                    for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                    {
                        var property = dataContractPropertiesSets[i];

                        if (String.IsNullOrEmpty(property.Source) && String.IsNullOrEmpty(property.Target))
                        {
                            continue;
                        }

                        lines.Add(new CodeLine(2, "{0} = {1}{2}", property.Target, property.Source, i < dataContractPropertiesSets.Count - 1 ? "," : String.Empty));
                    }

                    lines.Add(new CodeLine(1, "}};"));
                    lines.Add(new CodeLine());
                }
                else
                {
                    returnType = dbObject.GetSingularName();

                    lines.Add(new CodeLine("var query = DbContext.Set<{0}>().AsQueryable();", dbObject.GetSingularName()));
                    lines.Add(new CodeLine());

                }
            }

            var parameters = new List<ParameterDefinition>()
            {
                new ParameterDefinition("Int32", "pageSize", "10"),
                new ParameterDefinition("Int32", "pageNumber", "0")
            };

            if (tableCast != null)
            {
                if (tableCast.ForeignKeys.Count == 0)
                {
                    lines.Add(new CodeLine("return Paging<{0}>(pageSize, pageNumber);", dbObject.GetSingularName()));
                }
                else
                {
                    var resolver = new ClrTypeResolver() as ITypeResolver;

                    for (var i = 0; i < tableCast.ForeignKeys.Count; i++)
                    {
                        var foreignKey = tableCast.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = tableCast.Columns.First(item => item.Name == foreignKey.Key[0]);

                            var parameterName = NamingConvention.GetParameterName(column.Name);

                            parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), parameterName, "null"));

                            if (column.IsString())
                            {
                                lines.Add(new CodeLine("if (!String.IsNullOrEmpty({0}))", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{{"));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}}"));
                                lines.Add(new CodeLine());
                            }
                            else
                            {
                                lines.Add(new CodeLine("if ({0}.HasValue)", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{{"));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}}"));
                                lines.Add(new CodeLine());
                            }
                        }
                        else
                        {
                            // todo: add logic for composed foreign key
                        }
                    }

                    lines.Add(new CodeLine("return Paging(query, pageSize, pageNumber);"));
                }
            }

            return new MethodDefinition(String.Format("IQueryable<{0}>", returnType), String.Format("Get{0}", dbObject.GetPluralName()), parameters.ToArray())
            {
                Lines = lines
            };
        }

        public void AddGetByUniqueMethods(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var table = dbObject as Table;

            if (table == null)
            {
                table = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);
            }

            if (table == null)
            {
                return;
            }

            foreach (var unique in table.Uniques)
            {
                var expression = String.Format("item => {0}", String.Join(" && ", unique.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));

                Methods.Add(new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), String.Format("Get{0}By{1}Async", dbObject.GetSingularName(), String.Join("And", unique.Key.Select(item => NamingConvention.GetPropertyName(item)))), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>()
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", Project.Settings.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                    }
                });
            }
        }

        public MethodDefinition GetGetMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var table = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);

            var expression = String.Empty;

            if (table != null)
            {
                if (table.PrimaryKey == null)
                {
                    if (table.Identity != null)
                    {
                        expression = String.Format("item => item.{0} == entity.{0}", NamingConvention.GetPropertyName(table.Identity.Name));
                    }
                }
                else
                {
                    expression = String.Format("item => {0}", String.Join(" && ", table.PrimaryKey.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));
                }
            }

            return new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), String.Format("Get{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>()
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", Project.Settings.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                }
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var lines = new List<ILine>();

            var tableCast = dbObject as Table;

            if (tableCast != null)
            {
                if (tableCast.IsPrimaryKeyGuid())
                {
                    lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", NamingConvention.GetPropertyName(tableCast.PrimaryKey.Key[0])));
                    lines.Add(new CodeLine());
                }
            }

            lines.Add(new CodeLine("Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", String.Format("Add{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("Update(changes);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", String.Format("Update{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetDeleteMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            return new MethodDefinition("Task<Int32>", String.Format("Remove{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>()
                {
                    new CodeLine("Remove(entity);"),
                    new CodeLine(),
                    new CodeLine("return await CommitChangesAsync();")
                }
            };
        }
    }
}
