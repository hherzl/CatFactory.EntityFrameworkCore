using CatFactory.ObjectRelationalMapping;
using CatFactory.SqlServer;

namespace CatFactory.EntityFrameworkCore.Tests
{
    public static partial class Databases
    {
        public static Database School
            => new Database
            {
                Name = "school",
                DefaultSchema = "dbo",
                DatabaseTypeMaps = DatabaseTypeMapList.Definition,
                Tables =
                {
                    new Table
                    {
                        Schema = "dbo",
                        Name = "student",
                        Columns =
                        {
                            new Column { Name = "student_id", Type = "int" },
                            new Column { Name = "first_name", Type = "varchar", Length = 25 },
                            new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                            new Column { Name = "last_name", Type = "varchar", Length = 25 },
                            new Column { Name = "birth_date", Type = "datetime" },
                            new Column { Name = "gender", Type = "varchar", Length = 1 }
                        },
                        Identity = new Identity("student_id", 1, 1)
                    },
                    new Table
                    {
                        Schema = "dbo",
                        Name = "teacher",
                        Columns =
                        {
                            new Column { Name = "teacher_id", Type = "int" },
                            new Column { Name = "first_name", Type = "varchar", Length = 25 },
                            new Column { Name = "middle_name", Type = "varchar", Length = 25, Nullable = true },
                            new Column { Name = "last_name", Type = "varchar", Length = 25 },
                            new Column { Name = "birth_date", Type = "datetime" },
                            new Column { Name = "gender", Type = "varchar", Length = 1 }
                        },
                        Identity = new Identity("teacher_id", 1, 1)
                    },
                    new Table
                    {
                        Schema = "dbo",
                        Name = "course",
                        Columns =
                        {
                            new Column { Name = "course_id", Type = "int" },
                            new Column { Name = "course_name", Type = "varchar", Length = 25 },
                            new Column { Name = "course_description", Type = "varchar", Nullable = true }
                        },
                        Identity = new Identity("course_id", 1, 1)
                    },
                    new Table
                    {
                        Schema = "dbo",
                        Name = "course_teacher",
                        Columns =
                        {
                            new Column { Name = "course_teacher_id", Type = "int" },
                            new Column { Name = "course_id", Type = "int" },
                            new Column { Name = "teacher_id", Type = "int" },
                        },
                        Identity = new Identity("course_teacher_id", 1, 1)
                    }
                }
            }
            .AddDbObjectsFromTables()
            .AddDbObjectsFromViews()
            .SetPrimaryKeyToTables()
            .LinkTables();
    }
}
