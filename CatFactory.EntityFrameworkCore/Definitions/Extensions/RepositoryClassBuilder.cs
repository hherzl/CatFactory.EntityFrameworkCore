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
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryClassBuilder
    {
        public static RepositoryClassDefinition GetRepositoryClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var definition = new RepositoryClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Data.SqlClient",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Microsoft.EntityFrameworkCore"
                },
                Namespace = project.GetDataLayerRepositoriesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = projectFeature.GetRepositoryClassName(),
                BaseClass = "Repository",
                Implements =
                {
                    projectFeature.GetRepositoryInterfaceName()
                },
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public, new ParameterDefinition(project.GetDbContextName(project.Database), "dbContext"))
                    {
                        Invocation = "base(dbContext)"
                    }
                }
            };

            foreach (var table in project.Database.Tables)
            {
                definition.Namespaces
                    .AddUnique(projectFeature.Project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema));

                definition.Namespaces.AddUnique(project.GetDataLayerContractsNamespace());
            }

            foreach (var table in projectFeature.GetTables())
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(table);

                if (projectSelection.Settings.EntitiesWithDataContracts)
                    definition.Namespaces.AddUnique(project.GetDataLayerDataContractsNamespace());

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (string.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = projectFeature.Project.Database.FindTable(foreignKey.Child);

                        if (child != null)
                            definition.Namespaces.AddUnique(project.GetDataLayerDataContractsNamespace());
                    }
                }

                if (table.ForeignKeys.Count == 0)
                    definition.Methods.Add(GetGetAllMethodWithoutForeigns(projectFeature, projectSelection, table));
                else
                    definition.GetGetAllMethod(projectFeature, projectSelection, table);

                if (table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethod(projectFeature, projectSelection, table));

                foreach (var unique in table.Uniques)
                    definition.Methods.Add(GetGetByUniqueMethods(projectFeature, table, unique));

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            foreach (var view in projectFeature.GetViews())
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(view);

                if (projectSelection.Settings.EntitiesWithDataContracts)
                    definition.Namespaces.AddUnique(project.GetDataLayerDataContractsNamespace());

                definition.Methods.Add(GetGetAllMethod(projectFeature, view));

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            foreach (var tableFunction in project.Database.GetTableFunctions())
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(tableFunction);

                definition.Methods.Add(GetGetAllMethod(projectFeature, tableFunction));

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            foreach (var storedProcedure in project.Database.GetStoredProcedures())
            {
                var projectSelection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(storedProcedure);

                definition.Methods.Add(GetGetAllMethod(projectFeature, storedProcedure));

                if (projectSelection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            return definition;
        }

        private static void GetGetAllMethod(this CSharpClassDefinition definition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                returnType = project.GetDataContractName(table);

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

                var entityAlias = NamingConvention.GetCamelCase(project.GetEntityName(table));

                foreach (var column in table.Columns)
                {
                    var propertyName = project.GetPropertyName(table, column);

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

                    var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

                    foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                    {
                        var col = (Column)column;

                        var propertyName = project.GetPropertyName(foreignTable, col);

                        if (dataContractPropertiesSets.Where(item => string.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == string.Format("{0}.{1}", entityAlias, propertyName)).Count() == 0)
                        {
                            var target = string.Format("{0}{1}", project.GetEntityName(foreignTable), propertyName);

                            dataContractPropertiesSets.Add(new
                            {
                                IsForeign = true,
                                column.Type,
                                column.Nullable,
                                ObjectSource = foreignKeyAlias,
                                PropertySource = propertyName,
                                Target = target
                            });
                        }
                    }
                }

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, project.GetEntityName(table)));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyEntityName = project.GetDbSetPropertyName(foreignTable);

                    var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

                    if (projectFeature.Project.Database.HasDefaultSchema(foreignTable))
                        definition.Namespaces.AddUnique(project.GetEntityLayerNamespace());
                    else
                        definition.Namespaces.AddUnique(project.GetEntityLayerNamespace(foreignTable.Schema));

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

                            var x = project.CodeNamingConvention.GetPropertyName(foreignKey.Key.First());
                            var y = project.CodeNamingConvention.GetPropertyName(foreignTable.PrimaryKey.Key.First());

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
                        // todo: Add extension method to retrieve database type map by name

                        var dbTypeMap = projectFeature.Project.Database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == property.Type);

                        if (dbTypeMap == null)
                            throw new ObjectRelationMappingException(string.Format("There isn't mapping for '{0}' type", property.Type));

                        var clrType = dbTypeMap.GetClrType();

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
                lines.Add(new EmptyLine());
            }
            else
            {
                returnType = project.GetEntityName(table);

                var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

                returnType = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
                var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table) : project.GetFullDbSetPropertyName(table);

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = DbContext.{0}.AsQueryable();", dbSetName));

                lines.Add(new EmptyLine());
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

                        var parameterName = project.GetParameterName(column);

                        parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), parameterName, "null"));

                        if (projectFeature.Project.Database.ColumnIsDateTime(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsNumber(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsString(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0} != null)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.Warning("Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            definition.Methods.Add(new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = string.Format("IQueryable<{0}>", returnType),
                Name = project.GetGetAllRepositoryMethodName(table),
                Parameters = parameters,
                Lines = lines
            });
        }

        private static MethodDefinition GetGetAllMethodWithoutForeigns(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

            var genericTypeName = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
            var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table) : project.GetFullDbSetPropertyName(table);

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = string.Format("IQueryable<{0}>", genericTypeName),
                Name = project.GetGetAllRepositoryMethodName(table),
                Lines =
                {
                    new CodeLine("return DbContext.{0};", dbSetName)
                }
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, IView view)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var primaryKeys = project.Database
                .Tables
                .Where(item => item.PrimaryKey != null)
                .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First())
                .ToList();

            var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            var parameters = new List<ParameterDefinition>();

            foreach (var pk in result)
            {
                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(pk), project.GetParameterName(pk), "null"));
            }

            var lines = new List<ILine>();

            var existingTables = project.Database.Tables.Count(item => item.Name == view.Name);

            var genericTypeName = existingTables == 0 ? project.GetEntityName(view) : project.GetFullEntityName(view);
            var dbSetName = existingTables == 0 ? project.GetDbSetPropertyName(view) : project.GetFullDbSetPropertyName(view);

            if (parameters.Count == 0)
            {
                lines.Add(new CodeLine("return DbContext.{0};", dbSetName));
            }
            else
            {
                lines.Add(new CodeLine("var query = DbContext.{0}.AsQueryable();", dbSetName));
                lines.Add(new EmptyLine());

                foreach (var pk in result)
                {
                    if (project.Database.ColumnIsNumber(pk))
                    {
                        lines.Add(new CodeLine("if ({0}.HasValue)", project.CodeNamingConvention.GetParameterName(pk.Name)));
                        lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.CodeNamingConvention.GetPropertyName(pk.Name), project.CodeNamingConvention.GetParameterName(pk.Name)));
                        lines.Add(new EmptyLine());
                    }
                    else if (project.Database.ColumnIsString(pk))
                    {
                        lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", project.CodeNamingConvention.GetParameterName(pk.Name)));
                        lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.CodeNamingConvention.GetPropertyName(pk.Name), project.CodeNamingConvention.GetParameterName(pk.Name)));
                        lines.Add(new EmptyLine());
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Type = string.Format("IQueryable<{0}>", genericTypeName),
                Name = project.GetGetAllRepositoryMethodName(view),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, TableFunction tableFunction)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var parameters = new List<ParameterDefinition>();

            var lines = new List<ILine>()
            {
                new CommentLine(" Create query for table function")
            };

            lines.Add(new CodeLine("var query = new"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "Text = \" select {0} from {1}({2}) \",", string.Join(", ", tableFunction.Columns.Select(item => item.Name)), project.Database.GetFullName(tableFunction), string.Join(", ", tableFunction.Parameters.Select(item => item.Name))));

            if (tableFunction.Parameters.Count == 0)
            {
                lines.Add(new CodeLine(1, "Parameters = new object[] {}"));
            }
            else
            {
                lines.Add(new CodeLine(1, "Parameters = new[]"));
                lines.Add(new CodeLine(1, "{"));

                foreach (var parameter in tableFunction.Parameters)
                {
                    lines.Add(new CodeLine(2, "new SqlParameter(\"{0}\", {1}),", parameter.Name, project.CodeNamingConvention.GetParameterName(parameter.Name)));

                    parameters.Add(new ParameterDefinition(project.Database.ResolveDatabaseType(parameter), project.CodeNamingConvention.GetParameterName(parameter.Name)));
                }

                lines.Add(new CodeLine(1, "}"));
            }

            lines.Add(new CodeLine("};"));

            lines.Add(new EmptyLine());

            lines.Add(new ReturnLine("await DbContext"));
            lines.Add(new CodeLine(1, ".Query<{0}>()", project.GetEntityResultName(tableFunction)));
            lines.Add(new CodeLine(1, ".FromSql(query.Text, query.Parameters)"));
            lines.Add(new CodeLine(1, ".ToListAsync();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<IEnumerable<{0}>>", project.GetEntityResultName(tableFunction)),
                Name = project.GetGetAllRepositoryMethodName(tableFunction),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, StoredProcedure storedProcedure)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var parameters = new List<ParameterDefinition>();

            var lines = new List<ILine>()
            {
                new CommentLine(" Create query for table function")
            };

            lines.Add(new CodeLine("var query = new"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "Text = \" exec {0} {1} \",", project.Database.GetFullName(storedProcedure), string.Join(", ", storedProcedure.Parameters.Select(item => item.Name))));

            if (storedProcedure.Parameters.Count == 0)
            {
                lines.Add(new CodeLine(1, "Parameters = new object[] {}"));
            }
            else
            {
                lines.Add(new CodeLine(1, "Parameters = new[]"));
                lines.Add(new CodeLine(1, "{"));

                foreach (var parameter in storedProcedure.Parameters)
                {
                    lines.Add(new CodeLine(2, "new SqlParameter(\"{0}\", {1}),", parameter.Name, project.CodeNamingConvention.GetParameterName(parameter.Name)));

                    parameters.Add(new ParameterDefinition(project.Database.ResolveDatabaseType(parameter), project.CodeNamingConvention.GetParameterName(parameter.Name)));
                }

                lines.Add(new CodeLine(1, "}"));
            }

            lines.Add(new CodeLine("};"));

            lines.Add(new EmptyLine());

            lines.Add(new ReturnLine("await DbContext"));
            lines.Add(new CodeLine(1, ".Query<{0}>()", project.GetEntityResultName(storedProcedure)));
            lines.Add(new CodeLine(1, ".FromSql(query.Text, query.Parameters)"));
            lines.Add(new CodeLine(1, ".ToListAsync();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<IEnumerable<{0}>>", project.GetEntityResultName(storedProcedure)),
                Name = project.GetGetAllRepositoryMethodName(storedProcedure),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetByUniqueMethods(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var selection = project.GetSelection(table);

            var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(item)))));

            var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

            var genericTypeName = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
            var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table) : project.GetFullDbSetPropertyName(table);

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                Name = project.GetGetByUniqueRepositoryMethodName(table, unique),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines =
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", dbSetName, expression)
                }
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Empty;

            if (table.Identity == null)
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(item)))));
            else
                expression = string.Format("item => item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(table.Identity.Name));

            var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

            var genericTypeName = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
            var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table) : project.GetFullDbSetPropertyName(table);

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                var lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}", dbSetName)
                };

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignKey == null)
                        continue;

                    lines.Add(new CodeLine(1, ".Include(p => p.{0})", foreignKey.GetParentNavigationProperty(foreignTable, project).Name));
                }

                lines.Add(new CodeLine(1, ".FirstOrDefaultAsync({0});", expression));

                return new MethodDefinition
                {
                    AccessModifier = AccessModifier.Public,
                    IsAsync = true,
                    Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                    Name = project.GetGetRepositoryMethodName(table),
                    Parameters =
                    {
                        new ParameterDefinition(project.GetEntityName(table), "entity")
                    },
                    Lines = lines
                };
            }
            else
            {
                return new MethodDefinition
                {
                    AccessModifier = AccessModifier.Public,
                    IsAsync = true,
                    Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                    Name = project.GetGetRepositoryMethodName(table),
                    Parameters =
                    {
                        new ParameterDefinition(project.GetEntityName(table), "entity")
                    },
                    Lines =
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", dbSetName, expression)
                    }
                };
            }
        }
    }
}
