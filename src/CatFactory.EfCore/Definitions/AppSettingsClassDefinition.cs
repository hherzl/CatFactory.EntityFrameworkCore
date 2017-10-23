//using CatFactory.DotNetCore;
//using CatFactory.OOP;

//namespace CatFactory.EfCore.Definitions
//{
//    public static class AppSettingsClassDefinition
//    {
//        public static CSharpClassDefinition GetAppSettingsClassDefinition(this EfCoreProject project)
//        {
//            var classDefinition = new CSharpClassDefinition();

//            classDefinition.Namespaces.Add("System");

//            classDefinition.Namespace = project.GetDataLayerNamespace();

//            classDefinition.Name = "AppSettings";

//            classDefinition.Properties.Add(new PropertyDefinition("String", "ConnectionString"));

//            return classDefinition;
//        }
//    }
//}
