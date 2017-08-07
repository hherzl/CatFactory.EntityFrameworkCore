using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class AppSettingsClassDefinition : CSharpClassDefinition
    {
        public AppSettingsClassDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public override void Init()
        {
            Namespace = Project.GetDataLayerNamespace();

            Namespaces.Add("System");

            Name = "AppSettings";

            Properties.Add(new PropertyDefinition("String", "ConnectionString"));
        }
    }
}
