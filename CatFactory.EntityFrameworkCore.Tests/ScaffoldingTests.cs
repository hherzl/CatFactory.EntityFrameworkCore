using CatFactory.SqlServer;
using CatFactory.SqlServer.CodeFactory;
using CatFactory.SqlServer.ObjectRelationalMapping;
using Xunit;

namespace CatFactory.EntityFrameworkCore.Tests;

public class ScaffoldingTests
{
    [Fact]
    public void ExportXavierSchoolScript()
    {
        var database = SqlServerDatabase.CreateWithDefaults("XavierSchool");

        database.AddDefaultTypeMapFor(typeof(string), "nvarchar");
        database.AddDefaultTypeMapFor(typeof(DateTime), "datetime");

        var now = DateTime.Now;

        var teacher = database
            .DefineEntity(new
            {
                Id = 0,
                GivenName = "",
                MiddleName = "",
                FamilyName = "",
                BirthDate = now,
                Gender = "",
                Phone = "",
                Email = ""
            })
            .SetNaming("Teacher")
            .SetColumnFor(p => p.GivenName, 25)
            .SetColumnFor(p => p.MiddleName, 25, true)
            .SetColumnFor(p => p.FamilyName, 25)
            .SetIdentity(p => p.Id)
            .SetPrimaryKey(e => e.Id)
            ;

        var classroom = database
            .DefineEntity(new
            {
                Id = (short)0,
                Code = "",
                Name = ""
            })
            .SetNaming("Classroom")
            .SetColumnFor(e => e.Code, 25)
            .SetColumnFor(e => e.Name, 100)
            .SetIdentity(e => e.Id)
            .SetPrimaryKey(e => e.Id)
            .AddUnique(e => e.Code)
            .AddUnique(e => e.Name)
            ;

        var course = database
            .DefineEntity(new
            {
                Id = 0,
                TeacherId = 0,
                ClassroomId = (short)0,
                Name = "",
                Description = ""
            })
            .SetNaming("Course")
            .SetColumnFor(e => e.Name, 100)
            .SetIdentity(e => e.Id, 1000, 1000)
            .SetPrimaryKey(e => e.Id)
            .AddUnique(e => e.Name)
            .AddForeignKey(e => e.TeacherId, teacher.Table)
            .AddForeignKey(e => e.ClassroomId, classroom.Table)
            ;

        var student = database
            .DefineEntity(new
            {
                Id = 0,
                GivenName = "",
                MiddleName = "",
                FamilyName = "",
                BirthDate = now,
                Gender = "",
                Phone = "",
                Email = ""
            })
            .SetNaming("Student")
            .SetColumnFor(p => p.GivenName, 25)
            .SetColumnFor(p => p.MiddleName, 25, true)
            .SetColumnFor(p => p.FamilyName, 25)
            .SetIdentity(p => p.Id)
            .SetPrimaryKey(e => e.Id)
            ;

        student
            .AddExtendedProp(p => p.GivenName, "MS_Description", "Given name")
            .AddExtendedProp(p => p.MiddleName, "MS_Description", "Middle name")
            .AddExtendedProp(p => p.FamilyName, "MS_Description", "Family name")
            ;

        var courseStudent = database
            .DefineEntity(new
            {
                Id = 0,
                CourseId = 0,
                StudentId = 0,
                EnrollmentDate = now
            })
            .SetNaming("CourseStudent")
            .SetIdentity(p => p.Id)
            .SetPrimaryKey(p => p.Id)
            .AddUnique(p => new { p.CourseId, p.StudentId })
            .AddForeignKey(p => p.CourseId, course.Table)
            .AddForeignKey(p => p.StudentId, student.Table)
            ;

        SqlServerDatabaseScriptCodeBuilder.CreateScript(database, @"C:\Temp\CatFactory.SqlServer", true, true);
    }

    [Fact]
    public async Task ScaffoldingXavierSchoolAsync()
    {
        var database = await SqlServerDatabaseFactory
            .ImportAsync("server=(local); database=XavierSchool; integrated security=yes; TrustServerCertificate=True;", "dbo.sysdiagrams");

        var project = EntityFrameworkCoreProject
            .CreateForV9x("XavierSchool", database, @"C:\Temp\CatFactory.EntityFrameworkCore\XavierSchool");

        project.GlobalSelection(settings =>
        {
            settings.ForceOverwrite = true;
            settings.DeclareNavigationProperties = true;
            settings.DeclareNavigationPropertiesAsVirtual = true;
            settings.AddConfigurationForForeignKeysInFluentAPI = true;
            settings.UseApplyConfigurationsFromAssemblyMethod = true;
        });

        //project.BuildFeatures();
        //project.ScaffoldDomain().ScaffoldApplication().ScaffoldInfrastructure(settings => settings.UseEntityFrameworkCore()).ScaffoldSharedKernel();
    }

    [Fact]
    public async Task ScaffoldingCleanArchitectureSolution()
    {
        var database = await SqlServerDatabaseFactory
            .ImportAsync("server=(local); database=XavierSchool; integrated security=yes; TrustServerCertificate=True;", "dbo.sysdiagrams");

        var solution = new CleanArchitectureSolution("XavierSchool", @"C:\Temp\CatFactory.EntityFrameworkCore\XavierSchool", database);

        // TODO: scaffold DbContext queries: entities by primary key, unique and foreign key (add flags for tracking and includes)

        solution
            .ScaffoldDomain()
            .ScaffoldApplication();
    }
}
