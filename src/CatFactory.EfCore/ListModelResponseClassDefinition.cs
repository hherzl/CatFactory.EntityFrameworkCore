using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class ListModelResponseClassDefinition : CSharpClassDefinition
    {
        public ListModelResponseClassDefinition()
        {
            Name = "ListModelResponse<TEntity>";

            Implements.Add("IListModelResponse<TEntity>");

            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            Properties.Add(new PropertyDefinition("IEnumerable<TEntity>", "Model"));
        }
    }
}
