using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class SingleModelResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public SingleModelResponseInterfaceDefinition()
        {
            Name = "ISingleModelResponse<TEntity>";

            Implements.Add("IResponse");

            Properties.Add(new PropertyDefinition("TEntity", "Model"));
        }
    }
}
