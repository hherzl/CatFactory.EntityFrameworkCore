using System;

namespace CatFactory.EfCore
{
    public class EfCoreProject : Project
    {
        public EfCoreProject()
        {
            NavigationPropertyEnumerableNamespace = "System.Collections.ObjectModel";
            NavigationPropertyEnumerableType = "Collection";
        }

        private ProjectNamespaces m_namespaces;

        public ProjectNamespaces Namespaces
        {
            get
            {
                return m_namespaces ?? (m_namespaces = new ProjectNamespaces());
            }
            set
            {
                m_namespaces = value;
            }
        }

        public Boolean UseDataAnnotations { get; set; }

        public Boolean DeclareDbSetPropertiesInDbContext { get; set; }

        public Boolean DeclareNavigationPropertiesAsVirtual { get; set; }

        public String NavigationPropertyEnumerableNamespace { get; set; }

        public String NavigationPropertyEnumerableType { get; set; }
    }
}
