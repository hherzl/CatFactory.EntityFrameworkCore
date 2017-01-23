using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class ResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public ResponseInterfaceDefinition()
        {
            Name = "IResponse";

            Namespaces.Add("System");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
        }
    }
}
