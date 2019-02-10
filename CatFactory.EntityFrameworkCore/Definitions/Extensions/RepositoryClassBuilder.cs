using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryClassBuilder
    {
        public static RepositoryClassDefinition GetRepositoryClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var definition = new RepositoryClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Microsoft.EntityFrameworkCore"
                },
                Namespace = efCoreProject.GetDataLayerRepositoriesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = projectFeature.GetClassRepositoryName(),
                BaseClass = "Repository",
                Implements =
                {
                    projectFeature.GetInterfaceRepositoryName()
                },
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public, new ParameterDefinition(efCoreProject.GetDbContextName(efCoreProject.Database), "dbContext"))
                    {
                        Invocation = "base(dbContext)"
                    }
                }
            };

            foreach (var table in efCoreProject.Database.Tables)
            {
                definition.Namespaces
                    .AddUnique(projectFeature.Project.Database.HasDefaultSchema(table) ? efCoreProject.GetEntityLayerNamespace() : efCoreProject.GetEntityLayerNamespace(table.Schema));

                definition.Namespaces.AddUnique(efCoreProject.GetDataLayerContractsNamespace());
            }

            var tables = projectFeature
                .Project
                .Database
                .Tables
                .Where(item => projectFeature.DbObjects.Select(dbo => dbo.FullName).Contains(item.FullName))
                .ToList();

            foreach (var table in tables)
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(table);

                if (projectSelection.Settings.EntitiesWithDataContracts)
                    definition.Namespaces.AddUnique(efCoreProject.GetDataLayerDataContractsNamespace());

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (string.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = projectFeature.Project.Database.FindTable(foreignKey.Child);

                        if (child != null)
                            definition.Namespaces.AddUnique(efCoreProject.GetDataLayerDataContractsNamespace());
                    }
                }

                if (table.ForeignKeys.Count == 0)
                    definition.GetGetAllMethodWithoutForeigns(projectFeature, projectSelection, table);
                else
                    definition.GetGetAllMethod(projectFeature, projectSelection, table);

                if (table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethod(projectFeature, projectSelection, table));

                foreach (var unique in table.Uniques)
                    definition.Methods.Add(GetGetByUniqueMethods(projectFeature, table, unique));

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            var views = projectFeature
                .Project
                .Database
                .Views
                .Where(item => projectFeature.DbObjects.Select(dbo => dbo.FullName).Contains(item.FullName))
                .ToList();

            foreach (var view in views)
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(view);

                if (projectSelection.Settings.EntitiesWithDataContracts)
                    definition.Namespaces.AddUnique(efCoreProject.GetDataLayerDataContractsNamespace());

                definition.GetGetAllMethod(projectFeature, view);

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            return definition;
        }

        private static void GetGetAllMethod(this CSharpClassDefinition definition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                returnType = efCoreProject.GetDataContractName(table);

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

                var entityAlias = NamingConvention.GetCamelCase(efCoreProject.GetEntityName(table));

                foreach (var column in table.Columns)
                {
                    var propertyName = column.GetPropertyName();

                    dataContractPropertiesSets.Add(new
                    {
                        IsForeign = false,
                        column.Type,
                        column.Nullable,
                        ObjectSource = entityAlias,
                        PropertySource = propertyName,
                        Target = propertyName
                    });
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyAlias = NamingConvention.GetCamelCase(efCoreProject.GetEntityName(foreignTable));

                    foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                    {
                        if (dataContractPropertiesSets.Where(item => string.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == string.Format("{0}.{1}", entityAlias, column.GetPropertyName())).Count() == 0)
                        {
                            var target = string.Format("{0}{1}", efCoreProject.GetEntityName(foreignTable), column.GetPropertyName());

                            dataContractPropertiesSets.Add(new
                            {
                                IsForeign = true,
                                column.Type,
                                column.Nullable,
                                ObjectSource = foreignKeyAlias,
                                PropertySource = column.GetPropertyName(),
                                Target = target
                            });
                        }
                    }
                }

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, efCoreProject.GetEntityName(table)));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyEntityName = efCoreProject.GetDbSetPropertyName(foreignTable);

                    var foreignKeyAlias = NamingConvention.GetCamelCase(efCoreProject.GetEntityName(foreignTable));

                    if (projectFeature.Project.Database.HasDefaultSchema(foreignTable))
                        definition.Namespaces.AddUnique(efCoreProject.GetEntityLayerNamespace());
                    else
                        definition.Namespaces.AddUnique(efCoreProject.GetEntityLayerNamespace(foreignTable.Schema));

                    if (foreignKey.Key.Count == 0)
                    {
                        lines.Add(new PreprocessorDirectiveLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                    }
                    else if (foreignKey.Key.Count == 1)
                    {
                        if (foreignTable == null)
                        {
                            lines.Add(LineHelper.Warning(" There isn't definition for '{0}' in your current database", foreignKey.References));
                        }
                        else
                        {
                            var column = table.Columns.FirstOrDefault(item => item.Name == foreignKey.Key.First());

                            var x = efCoreProject.CodeNamingConvention.GetPropertyName(foreignKey.Key.First());
                            var y = efCoreProject.CodeNamingConvention.GetPropertyName(foreignTable.PrimaryKey.Key.First());

                            if (column.Nullable)
                            {
                                lines.Add(new CodeLine(1, "join {0}Join in DbContext.{1} on {2}.{3} equals {0}Join.{4} into {0}Temp", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                lines.Add(new CodeLine(2, "from {0} in {0}Temp.DefaultIfEmpty()", foreignKeyAlias, entityAlias, x, y));
                            }
                            else
                            {
                                lines.Add(new CodeLine(1, "join {0} in DbContext.{1} on {2}.{3} equals {0}.{4}", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                            }
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine(1, "select new {0}", returnType));
                lines.Add(new CodeLine(1, "{"));

                for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                {
                    var property = dataContractPropertiesSets[i];

                    if (string.IsNullOrEmpty(property.ObjectSource) && string.IsNullOrEmpty(property.Target))
                        continue;

                    if (property.IsForeign)
                    {
                        var dbType = projectFeature.Project.Database.ResolveType(property.Type);

                        if (dbType == null)
                            throw new ObjectRelationMappingException(string.Format("There isn't mapping for '{0}' type", property.Type));

                        var clrType = dbType.GetClrType();

                        if (clrType.FullName == typeof(byte[]).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(byte[]) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(bool).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(bool?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(string).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? string.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(DateTime).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(TimeSpan).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(TimeSpan?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(byte).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(byte?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(short).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(short?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(int).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(int?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(long).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(long?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(decimal).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(decimal?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(double).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(double?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(float).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(float?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(Guid).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Guid?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(object) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
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
                returnType = efCoreProject.GetEntityName(table);

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = DbContext.{0}.AsQueryable();", efCoreProject.GetDbSetPropertyName(table)));

                lines.Add(new CodeLine());
            }

            var parameters = new List<ParameterDefinition>();

            if (table.ForeignKeys.Count == 0)
            {
                lines.Add(new CodeLine("return query;"));
            }
            else
            {
                for (var i = 0; i < table.ForeignKeys.Count; i++)
                {
                    var foreignKey = table.ForeignKeys[i];

                    if (foreignKey.Key.Count == 1)
                    {
                        var column = table.Columns.First(item => item.Name == foreignKey.Key.First());

                        var parameterName = efCoreProject.GetParameterName(column);

                        parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), parameterName, "null"));

                        if (projectFeature.Project.Database.ColumnIsDateTime(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsNumber(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsString(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine());
                        }
                        else
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0} != null)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine());
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.Warning("Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            definition.Methods.Add(new MethodDefinition(AccessModifier.Public, string.Format("IQueryable<{0}>", returnType), efCoreProject.GetGetAllRepositoryMethodName(table), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static void GetGetAllMethodWithoutForeigns(this CSharpClassDefinition definition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            definition.Methods.Add(new MethodDefinition(AccessModifier.Public, string.Format("IQueryable<{0}>", efCoreProject.GetEntityName(table)), efCoreProject.GetGetAllRepositoryMethodName(table))
            {
                Lines =
                {
                    new CodeLine("return DbContext.{0};", efCoreProject.GetDbSetPropertyName(table))
                }
            });
        }

        private static void GetGetAllMethod(this CSharpClassDefinition definition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, IView view)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            definition.Methods.Add(new MethodDefinition(AccessModifier.Public, string.Format("IQueryable<{0}>", efCoreProject.GetEntityName(view)), efCoreProject.GetGetAllRepositoryMethodName(view))
            {
                Lines =
                {
                    new CodeLine("return DbContext.{0};", efCoreProject.GetDbSetPropertyName(view))
                }
            });
        }

        private static MethodDefinition GetGetByUniqueMethods(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var selection = efCoreProject.GetSelection(table);

            var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", efCoreProject.CodeNamingConvention.GetPropertyName(item)))));

            return new MethodDefinition(AccessModifier.Public, string.Format("Task<{0}>", efCoreProject.GetEntityName(table)), efCoreProject.GetGetByUniqueRepositoryMethodName(table, unique), new ParameterDefinition(efCoreProject.GetEntityName(table), "entity"))
            {
                IsAsync = true,
                Lines =
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", efCoreProject.GetDbSetPropertyName(table), expression)
                }
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var efCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Empty;

            if (table.Identity == null)
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", efCoreProject.CodeNamingConvention.GetPropertyName(item)))));
            else
                expression = string.Format("item => item.{0} == entity.{0}", efCoreProject.CodeNamingConvention.GetPropertyName(table.Identity.Name));

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                var lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}", efCoreProject.GetDbSetPropertyName(table))
                };

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = efCoreProject.Database.FindTable(foreignKey.References);

                    if (foreignKey == null)
                        continue;

                    lines.Add(new CodeLine(1, ".Include(p => p.{0})", foreignKey.GetParentNavigationProperty(foreignTable, efCoreProject).Name));
                }

                lines.Add(new CodeLine(1, ".FirstOrDefaultAsync({0});", expression));

                return new MethodDefinition(AccessModifier.Public, string.Format("Task<{0}>", efCoreProject.GetEntityName(table)), efCoreProject.GetGetRepositoryMethodName(table), new ParameterDefinition(efCoreProject.GetEntityName(table), "entity"))
                {
                    IsAsync = true,
                    Lines = lines
                };
            }
            else
            {
                return new MethodDefinition(AccessModifier.Public, string.Format("Task<{0}>", efCoreProject.GetEntityName(table)), efCoreProject.GetGetRepositoryMethodName(table), new ParameterDefinition(efCoreProject.GetEntityName(table), "entity"))
                {
                    IsAsync = true,
                    Lines =
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", efCoreProject.GetDbSetPropertyName(table), expression)
                    }
                };
            }
        }
    }
}
