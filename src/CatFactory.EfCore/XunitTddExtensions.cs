using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class XunitTddExtensions
    {  
        public static CSharpClassDefinition GetTestClass(this CSharpClassDefinition classDefinition)
        {
            var testClass = new CSharpClassDefinition();

            testClass.Namespaces.Add("System");
            testClass.Namespaces.Add("System.Threading.Tasks");  
            testClass.Namespaces.Add("Xunit");

            testClass.Namespace = "Tests";
            testClass.Name = String.Format("{0}Test", classDefinition.Name);

            foreach (var method in classDefinition.Methods)
            {
                var lines = new List<ILine>();

                lines.Add(new CommentLine(" Arrange"));
                lines.Add(new CodeLine("var instance = new {0}();", classDefinition.Name));

                foreach (var parameter in method.Parameters)
                {
                    if (String.IsNullOrEmpty(parameter.DefaultValue))
                    {
                        lines.Add(new CodeLine("var {0} = default({1});", parameter.Name, parameter.Type));
                    }
                    else
                    {
                        lines.Add(new CodeLine("var {0} = {1};", parameter.Name, parameter.DefaultValue));
                    }
                }

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Act"));

                if (method.IsAsync)
                {
                    lines.Add(new CodeLine("var result = await instance.{0}({1});", method.Name, method.Parameters.Count == 0 ? String.Empty : String.Join(", ", method.Parameters.Select(item => item.Name))));
                }
                else
                {
                    lines.Add(new CodeLine("var result = instance.{0}({1});", method.Name, method.Parameters.Count == 0 ? String.Empty : String.Join(", ", method.Parameters.Select(item => item.Name))));
                }

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Assert"));

                testClass.Methods.Add(new MethodDefinition(method.IsAsync ? "Task" : "void", String.Format("{0}Test", method.Name))
                {
                    IsAsync = method.IsAsync,
                    Attributes = new List<MetadataAttribute>()
                    {
                        new MetadataAttribute("Fact")
                    },
                    Lines = lines
                });
            }

            return testClass;
        }
    }
}
