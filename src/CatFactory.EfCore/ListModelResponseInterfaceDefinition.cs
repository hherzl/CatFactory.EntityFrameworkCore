using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class ListModelResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public ListModelResponseInterfaceDefinition()
        {
            Name = "IListModelResponse<TEntity>";

            Implements.Add("IResponse");

            Namespaces.Add("System.Collections.Generic");

            Properties.Add(new PropertyDefinition("IEnumerable<TEntity>", "Model"));
        }
    }
}
