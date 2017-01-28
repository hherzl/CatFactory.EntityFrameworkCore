﻿using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class BaseRepositoryClassDefinition : CSharpClassDefinition
    {
        public BaseRepositoryClassDefinition(EfCoreProject project)
        {
            Project = project;

            Namespaces.Add("System");
            Namespaces.Add("System.Threading.Tasks");

            Name = "Repository";

            Fields.Add(new FieldDefinition("Boolean", "Disposed") { AccessModifier = AccessModifier.Protected });
            Fields.Add(new FieldDefinition(project.Database.GetDbContextName(), "DbContext") { AccessModifier = AccessModifier.Protected });

            Constructors.Add(new ClassConstructorDefinition()
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(project.Database.GetDbContextName(), "dbContext")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext = dbContext;")
                }
            });

            Methods.Add(new MethodDefinition("void", "Dispose")
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("if (!Disposed)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "if (DbContext != null)"),
                    new CodeLine(1, "{{"),
                    new CodeLine(2, "DbContext.Dispose();"),
                    new CodeLine(),
                    new CodeLine(2, "Disposed = true;"),
                    new CodeLine(1, "}}"),
                    new CodeLine("}}")
                }
            });

            Methods.Add(new MethodDefinition("Int32", "CommitChanges")
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return DbContext.SaveChanges();")
                }
            });

            Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync")
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return DbContext.SaveChangesAsync();")
                }
            });

        }

        public EfCoreProject Project { get; set; }

    }
}
