using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class DbContextClassDefinition : CSharpClassDefinition
    {
        public DbContextClassDefinition(Database db)
        {
            Namespaces = new List<String>()
            {
                "System",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.Extensions.Options"
            };

            Name = db.GetDbContextName();

            BaseClass = "Microsoft.EntityFrameworkCore.DbContext";

            Constructors.Add(new ClassConstructorDefinition()
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("IOptions<AppSettings>", "appSettings"),
                    new ParameterDefinition("IEntityMapper", "entityMapper")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("ConnectionString = appSettings.Value.ConnectionString;"),
                    new CodeLine("EntityMapper = entityMapper;")
                }
            });

            Properties.Add(new PropertyDefinition("String", "ConnectionString"));
            Properties.Add(new PropertyDefinition("IEntityMapper", "EntityMapper"));

            Methods.Add(GetOnConfiguringMethod());
            Methods.Add(GetOnModelCreatingMethod());

            if (DeclareDbSetProperties)
            {
                foreach (var table in db.Tables)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", table.GetEntityName()), table.GetEntityName()));
                }

                foreach (var view in db.Views)
                {
                    Properties.Add(new PropertyDefinition(String.Format("DbSet<{0}>", view.GetEntityName()), view.GetEntityName()));
                }
            }
        }

        public Boolean DeclareDbSetProperties { get; set; }

        public MethodDefinition GetOnConfiguringMethod()
        {
            return new MethodDefinition("void", "OnConfiguring")
            {
                ModifierAccess = ModifierAccess.Protected,
                Prefix = "override",
                Parameters = new List<ParameterDefinition>() { new ParameterDefinition("DbContextOptionsBuilder", "optionsBuilder") },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("optionsBuilder.UseSqlServer(ConnectionString);"),
                    new CodeLine(),
                    new CodeLine("base.OnConfiguring(optionsBuilder);")
                }
            };
        }

        public MethodDefinition GetOnModelCreatingMethod()
        {
            return new MethodDefinition("void", "OnModelCreating")
            {
                ModifierAccess = ModifierAccess.Protected,
                Prefix = "override",
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("ModelBuilder", "modelBuilder")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("EntityMapper.MapEntities(modelBuilder);"),
                    new CodeLine(),
                    new CodeLine("base.OnModelCreating(modelBuilder);")
                }
            };
        }
    }
}
