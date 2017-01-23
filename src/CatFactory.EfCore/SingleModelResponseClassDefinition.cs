using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class SingleModelResponseClassDefinition : CSharpClassDefinition
    {
        public SingleModelResponseClassDefinition()
        {
            Name = "SingleModelResponse<TEntity>";

            Implements.Add("ISingleModelResponse<TEntity>");

            Namespaces.Add("System");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            Properties.Add(new PropertyDefinition("TEntity", "Model"));
        }
    }
}
