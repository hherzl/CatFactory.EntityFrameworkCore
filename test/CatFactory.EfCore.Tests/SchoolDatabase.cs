using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.EfCore.Tests
{
    public static class SchoolDatabase
    {
        public static Database Mock
        {
            get
            {
                var db = new Database()
                {
                    Name = "school",
                    Tables = new List<Table>()
                    {
                        new Table
                        {
                            Schema = "dbo",
                            Name = "student",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "student_id", Type = "int", Nullable = false },
                                new Column { Name = "first_name", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                                new Column { Name = "last_name", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "birth_date", Type = "datetime",  Nullable = false },
                                new Column { Name = "gender", Type = "varchar", Length = 1, Nullable = false }
                            },
                            Identity = new Identity { Name = "student_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "teacher",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "teacher_id", Type = "int", Nullable = false },
                                new Column { Name = "first_name", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                                new Column { Name = "last_name", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "birth_date", Type = "datetime",  Nullable = false },
                                new Column { Name = "gender", Type = "varchar", Length = 1, Nullable = false }
                            },
                            Identity = new Identity { Name = "teacher_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "course",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "course_id", Type = "int", Nullable = false },
                                new Column { Name = "course_name", Type = "varchar", Length = 25, Nullable = false },
                                new Column { Name = "course_description", Type = "varchar", Nullable = true }
                            },
                            Identity = new Identity { Name = "course_id", Seed = 1, Increment = 1 }
                        },
                        new Table
                        {
                            Schema = "dbo",
                            Name = "course_teacher",
                            Columns = new List<Column>()
                            {
                                new Column { Name = "course_teacher_id", Type = "int", Nullable = false },
                                new Column { Name = "course_id", Type = "int", Nullable = false },
                                new Column { Name = "teacher_id", Type = "int", Nullable = false },
                            },
                            Identity = new Identity { Name = "course_teacher_id", Seed = 1, Increment = 1 }
                        }
                    }
                };

                foreach (var item in db.Tables)
                {
                    db.DbObjects.Add(new DbObject { Schema = item.Schema, Name = item.Name, Type = "USER_TABLE" });
                }

                foreach (var item in db.Views)
                {
                    db.DbObjects.Add(new DbObject { Schema = item.Schema, Name = item.Name, Type = "VIEW" });
                }

                db.SetPrimaryKeyToTables();

                db.LinkTables();

                return db;
            }
        }
    }
}
