# CatFactory.EntityFrameworkCore ==^^==

This is the *CatFactory* package for *Entity Framework Core*.

## What Is CatFactory?

CatFactory is a scaffolding engine for .NET Core built with C#.

## How does it Works?

The concept behind CatFactory is to import an existing database from SQL Server instance and then to scaffold a target technology.

We can also replace the database from SQL Server instance with an in-memory database.

The flow to import an existing database is:

1. Create Database Factory
2. Import Database
3. Create instance of Project (Entity Framework Core, Dapper, etc)
4. Build Features (One feature per schema)
5. Scaffold objects, these methods read all objects from database and create instances for code builders

## Code Snippet

```csharp
// Create database factory
var databaseFactory = new SqlServerDatabaseFactory
{
	DatabaseImportSettings = new DatabaseImportSettings
	{
		ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
		ImportTableFunctions = true,
		Exclusions =
		{
			"dbo.sysdiagrams",
			"dbo.fn_diagramobjects"
		}
	}
};

// Import database
var database = databaseFactory.Import();

// Create instance of Entity Framework Core project
var project = new EntityFrameworkCoreProject
{
	Name = "OnlineStore.Core",
	Database = database,
	OutputDirectory = @"C:\Projects\OnlineStore.Core"
};

// Apply settings for Entity Framework Core project
project.GlobalSelection(settings =>
{
	settings.ForceOverwrite = true;
	settings.ConcurrencyToken = "Timestamp";
	settings.AuditEntity = new AuditEntity
	{
		CreationUserColumnName = "CreationUser",
		CreationDateTimeColumnName = "CreationDateTime",
		LastUpdateUserColumnName = "LastUpdateUser",
		LastUpdateDateTimeColumnName = "LastUpdateDateTime"
	};
});

project.Selection("Sales.OrderHeader", settings => settings.EntitiesWithDataContracts = true);

// Build features for project, group all entities by schema into a feature
project.BuildFeatures();

// Add event handlers to before and after of scaffold

project.ScaffoldingDefinition += (source, args) =>
{
	// Add code to perform operations with code builder instance before to create code file
};

project.ScaffoldedDefinition += (source, args) =>
{
	// Add code to perform operations after of create code file
};

// Scaffolding =^^=
project
	.ScaffoldEntityLayer()
	.ScaffoldDataLayer();
```

Also these technologies are supported:

+ [`ASP.NET Core`](https://github.com/hherzl/CatFactory.AspNetCore)
+ [`Dapper`](https://github.com/hherzl/CatFactory.Dapper)

## Database Objects

|Object|Supported|
|------|---------|
|Table|Yes|
|View|Yes|
|Scalar Function|Yes|
|Table Function|Yes|
|Stored Procedures|Yes|
|Sequences|Not yet|

## Roadmap

There will be a lot of improvements for CatFactory on road:

* Scaffolding Services Layer
* Dapper Integration for ASP.NET Core
* MD files
* Scaffolding C# Client for ASP.NET Web API
* Scaffolding Unit Tests for ASP.NET Core
* Scaffolding Integration Tests for ASP.NET Core
* Scaffolding Angular

## Concepts behind CatFactory

### Database Type Map

One of things I don't like to get equivalent between SQL data type for CLR is use magic strings, after of review the more "fancy" way to resolve a type equivalence is to have a class that allows to know the equivalence between SQL data type and CLR type.

This concept was created from this matrix: [`SQL Server Data Type Mappings`](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings).

Using this matrix as reference, now CatFactory has a class named DatabaseTypeMap. Database class contains a property with all mappings named DatebaseTypeMaps, so this property is filled by Import feature for SQL Server package.

```csharp
public class DatabaseTypeMap
{
    public string DatabaseType { get; set; }

    public bool AllowsLengthInDeclaration { get; set; }

    public bool AllowsPrecInDeclaration { get; set; }

    public bool AllowsScaleInDeclaration { get; set; }

    public string ClrFullNameType { get; set; }

    public bool HasClrFullNameType { get; }

    public string ClrAliasType { get; set; }

    public bool HasClrAliasType { get; }

    public bool AllowClrNullable { get; set; }

    public DbType DbTypeEnum { get; set; }

    public bool IsUserDefined { get; set; }

    public string ParentDatabaseType { get; set; }

    public string Collation { get; set; }
}
```

DatabaseTypeMap is the class to represent database type definition, for database instance we need to create a collection of DatabaseTypeMap class to have a matrix to resolve data types.

Suppose there is a class with name DatabaseTypeMapList, this class has a property to get data types. Once we have imported an existing database we can resolve data types:

Resolve without extension methods:

```csharp
// Import database
var database = SqlServerDatabaseFactory
	.Import("server=(local);database=OnlineStore;integrated security=yes;");
				
// Resolve CLR type
var mapsForString = database.DatabaseTypeMaps.Where(item => item.GetClrType() == typeof(string)).ToList();

// Resolve SQL Server type
var mapForVarchar = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == "varchar");
```

SQL Server allows to define data types, suppose the database instance has a data type defined by user with name Flag, Flag data type is a bit, bool in C#. Import method retrieve user data types, so in DatabaseTypeMaps collection we can search the parent data type for Flag:

### Project Selection

A project selection is a limit to apply settings for objects that match with pattern.

GlobalSelection is the default selection for project, contains a default instance of settings.

Patterns:

|Pattern|Scope|
|-------|-----|
|Sales.OrderHeader|Applies for specific object with name Sales.OrderHeader|
|Sales.\*|Applies for all objects inside of Sales schema|
|\*.OrderHeader|Applies for all objects with name Order with no matter schema|
|\*.\*|Applies for all objects, this is the global selection|

Sample:

```csharp
// Apply settings for Project
project.GlobalSelection(settings =>
{
    settings.ForceOverwrite = true;
    settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
    settings.ConcurrencyToken = "Timestamp";
});

// Apply settings for specific object
project.Select("Sales.OrderHeader", settings =>
{
    settings.EntitiesWithDataContracts = true;
});
```

### Event Handlers to Scaffold

In order to provide a more flexible way to scaffold, there are two delegates in CatFactory, one to perform an action before of scaffolding and another one to handle and action after of scaffolding.

```csharp
// Add event handlers to before and after of scaffold

project.ScaffoldingDefinition += (source, args) =>
{
    // Add code to perform operations with code builder instance before to create code file
};

project.ScaffoldedDefinition += (source, args) =>
{
    // Add code to perform operations after of create code file
};
```

## Quick Starts

[`Scaffolding Dapper with CatFactory`](https://www.codeproject.com/Articles/1213355/Scaffolding-Dapper-with-CatFactory)

[`Scaffolding View Models with CatFactory`](https://www.codeproject.com/Tips/1164636/Scaffolding-View-Models-with-CatFactory)

[`Scaffolding Entity Framework Core 2 with CatFactory`](https://www.codeproject.com/Articles/1160615/Scaffolding-Entity-Framework-Core-with-CatFactory)

[`Scaffolding ASP.NET Core 2 with CatFactory`](https://www.codeproject.com/Tips/1229909/Scaffolding-ASP-NET-Core-with-CatFactory)

[`Scaffolding TypeScript with CatFactory`](https://www.codeproject.com/Tips/1166380/Scaffolding-TypeScript-with-CatFactory)
