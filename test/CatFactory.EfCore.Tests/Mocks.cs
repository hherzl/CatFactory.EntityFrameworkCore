using System;
using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.EfCore.Tests
{
    public class Mocks
    {
        public static Database SalesDatabase
        {
            get
            {
                var db = new Database()
                {
                    Name = "Sales",
                    Tables = new List<Table>()
                    {
                        new Table
                        {
                            Schema = "HumanResources",
                            Name = "Employee",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "EmployeeID", Type = "int", Nullable = false },
                                new Column { Name = "FirstName", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "MiddleName", Type = "varchar", Length = 25, Nullable = true },
                                new Column { Name = "LastName", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "BirthDate", Type = "datetime", Nullable = false }
                            },
                            Identity = new Identity { Name = "EmployeeID", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "Production",
                            Name = "Product",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "ProductID", Type = "int", Nullable = false },
                                new Column { Name = "ProductName", Type = "varchar", Length = 100, Nullable = false },
                                new Column { Name = "Description", Type = "varchar", Length = 255, Nullable = true }
                            },
                            Identity = new Identity { Name = "ProductID", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "Sales",
                            Name = "Order",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "OrderID", Type = "int", Nullable = false },
                                new Column { Name = "OrderDate", Type = "datetime", Nullable = false },
                                new Column { Name = "CustomerID", Type = "int", Nullable = false },
                                new Column { Name = "EmployeeID", Type = "int", Nullable = false },
                                new Column { Name = "ShipperID", Type = "datetime", Nullable = false },
                                new Column { Name = "Comments", Type = "varchar", Length = 255, Nullable = true }
                            },
                            Identity = new Identity { Name = "OrderID", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "Sales",
                            Name = "OrderDetail",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "OrderID", Type = "int", Nullable = false },
                                new Column { Name = "ProductID", Type = "int", Nullable = false },
                                new Column { Name = "ProductName", Type = "varchar", Length = 255, Nullable = false },
                                new Column { Name = "UnitPrice", Type = "decimal", Length = 8, Prec = 4, Nullable = false },
                                new Column { Name = "Quantity", Type = "int", Nullable = false },
                                new Column { Name = "Total", Type = "decimal", Length = 8, Prec = 4, Nullable = false }
                            },
                            PrimaryKey = new PrimaryKey(new String[] { "OrderID", "ProductID" })
                        }
                    },
                    Views = new List<View>()
                    {
                        new View
                        {
                            Schema = "Sales",
                            Name = "OrderSummary",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "OrderID", Type = "int", Nullable = false },
                                new Column { Name = "CustomerName", Type = "varchar", Length = 100, Nullable = false },
                                new Column { Name = "OrderDate", Type = "datetime", Nullable = false }
                            }
                        }
                    }
                };

                foreach (var item in db.Tables)
                {
                    db.DbObjects.Add(new DbObject { Schema = item.Schema, Name = item.Name, Type = "table" });
                }

                foreach (var item in db.Views)
                {
                    db.DbObjects.Add(new DbObject { Schema = item.Schema, Name = item.Name, Type = "view" });
                }

                db.AddPrimaryKeyToTables();

                return db;
            }
        }
    }
}
