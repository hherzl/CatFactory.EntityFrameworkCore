using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class RepositoryClassDefinition
    {
        private static ICodeNamingConvention NamingConvention;

        static RepositoryClassDefinition()
        {
            NamingConvention = new DotNetNamingConvention();
        }

        public static CSharpClassDefinition GetRepositoryClassDefinition(this ProjectFeature projectFeature)
        {
            var efCoreProject = projectFeature.GetEfCoreProject();

            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add("System.Threading.Tasks");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            foreach (var table in efCoreProject.Database.Tables)
            {
                classDefinition.Namespaces.AddUnique(table.HasDefaultSchema() ? efCoreProject.GetEntityLayerNamespace() : efCoreProject.GetEntityLayerNamespace(table.Schema));

                classDefinition.Namespaces.AddUnique(efCoreProject.GetDataLayerContractsNamespace());
            }

            classDefinition.Namespace = efCoreProject.GetDataLayerRepositoriesNamespace();

            classDefinition.Name = projectFeature.GetClassRepositoryName();

            classDefinition.BaseClass = "Repository";

            classDefinition.Implements.Add(projectFeature.GetInterfaceRepositoryName());

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(projectFeature.Project.Database.GetDbContextName(), "dbContext"))
            {
                Invocation = "base(dbContext)"
            });

            var tables = projectFeature.Project.Database.Tables.Where(item => projectFeature.DbObjects.Select(dbo => dbo.FullName).Contains(item.FullName)).ToList();

            foreach (var table in tables)
            {
                if (efCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
                {
                    classDefinition.Namespaces.AddUnique(efCoreProject.GetDataLayerDataContractsNamespace());
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (string.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = projectFeature.Project.Database.FindTableBySchemaAndName(foreignKey.Child);

                        if (child != null)
                        {
                            classDefinition.Namespaces.AddUnique(efCoreProject.GetDataLayerDataContractsNamespace());
                        }
                    }
                }

                classDefinition.GetGetAllMethod(projectFeature, table);

                classDefinition.AddGetByUniqueMethods(projectFeature, table);

                classDefinition.Methods.Add(GetGetMethod(projectFeature, table));
                classDefinition.Methods.Add(GetAddMethod(projectFeature, table));
                classDefinition.Methods.Add(GetUpdateMethod(projectFeature, table));
                classDefinition.Methods.Add(GetRemoveMethod(projectFeature, table));
            }

            return classDefinition;
        }

        private static void GetGetAllMethod(this CSharpClassDefinition classDefinition, ProjectFeature projectFeature, IDbObject dbObject)
        {
            var efCoreProject = projectFeature.GetEfCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            var table = dbObject as ITable;

            if (table == null)
            {
                returnType = dbObject.GetSingularName();

                lines.Add(new CodeLine("return query;"));
            }
            else
            {
                if (efCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
                {
                    var entityAlias = CatFactory.NamingConvention.GetCamelCase(table.GetEntityName());

                    returnType = table.GetDataContractName();

                    var dataContractPropertiesSets = new[]
                    {
                        new
                        {
                            IsForeign = false,
                            Type = string.Empty,
                            Nullable = false,
                            ObjectSource = string.Empty,
                            PropertySource = string.Empty,
                            Target = string.Empty
                        }
                    }.ToList();

                    foreach (var column in table.Columns)
                    {
                        var propertyName = column.GetPropertyName();

                        dataContractPropertiesSets.Add(new
                        {
                            IsForeign = false,
                            Type = column.Type,
                            Nullable = column.Nullable,
                            ObjectSource = entityAlias,
                            PropertySource = propertyName,
                            Target = propertyName
                        });
                    }

                    foreach (var foreignKey in table.ForeignKeys)
                    {
                        var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                        if (foreignTable == null)
                        {
                            continue;
                        }

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        foreach (var column in foreignTable?.GetColumnsWithOutPrimaryKey())
                        {
                            if (dataContractPropertiesSets.Where(item => string.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == string.Format("{0}.{1}", entityAlias, column.GetPropertyName())).Count() == 0)
                            {
                                var target = string.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                                dataContractPropertiesSets.Add(new
                                {
                                    IsForeign = true,
                                    Type = column.Type,
                                    Nullable = column.Nullable,
                                    ObjectSource = foreignKeyAlias,
                                    PropertySource = column.GetPropertyName(),
                                    Target = target
                                });
                            }
                        }
                    }

                    lines.Add(new CommentLine(" Get query from DbSet"));
                    lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, table.GetEntityName()));

                    foreach (var foreignKey in table.ForeignKeys)
                    {
                        var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                        if (foreignTable == null)
                        {
                            continue;
                        }

                        var foreignKeyEntityName = foreignTable.GetEntityName();

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        if (foreignTable.HasDefaultSchema())
                        {
                            classDefinition.Namespaces.AddUnique(efCoreProject.GetEntityLayerNamespace());
                        }
                        else
                        {
                            classDefinition.Namespaces.AddUnique(efCoreProject.GetEntityLayerNamespace(foreignTable.Schema));
                        }

                        if (foreignKey.Key.Count == 0)
                        {
                            lines.Add(new WarningLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                        }
                        else if (foreignKey.Key.Count == 1)
                        {
                            if (foreignTable == null)
                            {
                                lines.Add(new WarningLine(1, " There isn't definition for '{0}' in your current database", foreignKey.References));
                            }
                            else
                            {
                                var column = table.Columns.FirstOrDefault(item => item.Name == foreignKey.Key[0]);

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
                            // todo: add logic for foreign key with multiple key
                            lines.Add(new WarningLine(1, "// todo: add logic for foreign key with multiple key"));
                        }
                    }

                    lines.Add(new CodeLine(1, "select new {0}", returnType));
                    lines.Add(new CodeLine(1, "{"));

                    for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                    {
                        var property = dataContractPropertiesSets[i];

                        if (string.IsNullOrEmpty(property.ObjectSource) && string.IsNullOrEmpty(property.Target))
                        {
                            continue;
                        }

                        if (property.IsForeign)
                        {
                            if (property.Type.Contains("char"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? string.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else if (property.Type.Contains("date"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else if (property.Type.Contains("smallint"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int16?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else if (property.Type.Contains("bigint"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int64?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else if (property.Type.Contains("int"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int32?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else if (property.Type.Contains("decimal"))
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Decimal?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else
                        {
                            lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                    }

                    lines.Add(new CodeLine(1, "};"));
                    lines.Add(new CodeLine());
                }
                else
                {
                    returnType = dbObject.GetSingularName();

                    lines.Add(new CommentLine(" Get query from DbSet"));
                    lines.Add(new CodeLine("var query = DbContext.Set<{0}>().AsQueryable();", dbObject.GetSingularName()));
                    lines.Add(new CodeLine());

                }
            }

            var parameters = new List<ParameterDefinition>
            {
            };

            if (table != null)
            {
                if (table.ForeignKeys.Count == 0)
                {
                    lines.Add(new CodeLine("return query;"));
                }
                else
                {
                    var typeResolver = new ClrTypeResolver();

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.Columns.First(item => item.Name == foreignKey.Key[0]);

                            var parameterName = NamingConvention.GetParameterName(column.Name);

                            parameters.Add(new ParameterDefinition(typeResolver.Resolve(column.Type), parameterName, "null"));

                            if (column.IsString())
                            {
                                lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{"));
                                lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}"));
                                lines.Add(new CodeLine());
                            }
                            else
                            {
                                lines.Add(new CodeLine("if ({0}.HasValue)", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{"));
                                lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}"));
                                lines.Add(new CodeLine());
                            }
                        }
                        else
                        {
                            // todo: add logic for composed foreign key
                            lines.Add(new WarningLine(1, "// todo: add logic for foreign key with multiple key"));
                        }
                    }

                    lines.Add(new CodeLine("return query;"));
                }
            }

            classDefinition.Methods.Add(new MethodDefinition(string.Format("IQueryable<{0}>", returnType), dbObject.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static void AddGetByUniqueMethods(this CSharpClassDefinition classDefinition, ProjectFeature projectFeature, ITable table)
        {
            var efCoreProject = projectFeature.GetEfCoreProject();

            foreach (var unique in table.Uniques)
            {
                var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));

                classDefinition.Methods.Add(new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>()
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", efCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName()), expression)
                    }
                });
            }
        }

        private static MethodDefinition GetGetMethod(ProjectFeature projectFeature, ITable table)
        {
            var efCoreProject = projectFeature.GetEfCoreProject();

            var expression = string.Empty;

            if (table.PrimaryKey == null && table.Identity != null)
            {
                expression = string.Format("item => item.{0} == entity.{0}", NamingConvention.GetPropertyName(table.Identity.Name));
            }
            else
            {
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));
            }

            if (efCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
            {
                var lines = new List<ILine>();

                lines.Add(new CodeLine("return await DbContext.{0}", efCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName())));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignKey == null)
                    {
                        continue;
                    }

                    lines.Add(new CodeLine(1, ".Include(p => p.{0})", foreignKey.GetParentNavigationProperty(efCoreProject, foreignTable).Name));
                }

                lines.Add(new CodeLine(1, ".FirstOrDefaultAsync({0});", expression));

                return new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = lines
                };
            }
            else
            {
                return new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", efCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName()), expression)
                    }
                };
            }
        }

        private static MethodDefinition GetAddMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            if (table.IsPrimaryKeyGuid())
            {
                lines.Add(new CommentLine(" Set value for GUID"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Add entity in DbSet"));
            lines.Add(new CodeLine("Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Update entity in DbSet"));
            lines.Add(new CodeLine("Update(changes);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(ProjectFeature projectFeature, ITable table)
        {
            return new MethodDefinition("Task<Int32>", table.GetRemoveRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>()
                {
                    new CommentLine(" Remove entity from DbSet"),
                    new CodeLine("Remove(entity);"),
                    new CodeLine(),
                    new CommentLine(" Save changes through DbContext"),
                    new CodeLine("return await CommitChangesAsync();")
                }
            };
        }
    }
}
