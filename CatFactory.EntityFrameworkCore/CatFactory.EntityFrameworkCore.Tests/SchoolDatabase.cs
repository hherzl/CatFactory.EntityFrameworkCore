using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public static partial class Databases
    {
        public static Database School
        {
            get
            {
                var database = new Database
                {
                    Name = "school",
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            Schema = "dbo",
                            Name = "student",
                            Columns = new List<Column>
                            {
                                new Column { Name = "student_id", Type = "int" },
                                new Column { Name = "first_name", Type = "varchar", Length = 25 },
                                new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                                new Column { Name = "last_name", Type = "varchar", Length = 25 },
                                new Column { Name = "birth_date", Type = "datetime",  Nullable = false },
                                new Column { Name = "gender", Type = "varchar", Length = 1 }
                            },
                            Identity = new Identity { Name = "student_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "teacher",
                            Columns = new List<Column>
                            {
                                new Column { Name = "teacher_id", Type = "int" },
                                new Column { Name = "first_name", Type = "varchar", Length = 25 },
                                new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                                new Column { Name = "last_name", Type = "varchar", Length = 25 },
                                new Column { Name = "birth_date", Type = "datetime",  Nullable = false },
                                new Column { Name = "gender", Type = "varchar", Length = 1 }
                            },
                            Identity = new Identity { Name = "teacher_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "course",
                            Columns = new List<Column>
                            {
                                new Column { Name = "course_id", Type = "int" },
                                new Column { Name = "course_name", Type = "varchar", Length = 25 },
                                new Column { Name = "course_description", Type = "varchar", Nullable = true }
                            },
                            Identity = new Identity { Name = "course_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "course_teacher",
                            Columns = new List<Column>
                            {
                                new Column { Name = "course_teacher_id", Type = "int" },
                                new Column { Name = "course_id", Type = "int" },
                                new Column { Name = "teacher_id", Type = "int" },
                            },
                            Identity = new Identity { Name = "course_teacher_id", Seed = 1, Increment = 1 }
                        }
                    }
                };

                database.AddDbObjectsFromTables();
                database.AddDbObjectsFromViews();
                database.SetPrimaryKeyToTables();
                database.LinkTables();

                return database;
            }
        }
    }
}
