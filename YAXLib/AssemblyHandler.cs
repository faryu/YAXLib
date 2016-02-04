using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace YAXLib
{
    public class AssemblyHandler
    {
        public event EventHandler<ClassesLoadedEventArgs> ClassesLoaded;

        public static AssemblyHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lck)
                    {
                        if (_instance == null)
                            _instance = new AssemblyHandler();
                    }
                }
                return _instance;
            }
        }

        public static IList<Type> LoadedClasses
        {
            get
            {
                if (Instance._bUpdateRequired)
                    Instance.UpdateAssemblies();
                return Instance._lstLoadedClasses;
            }
        }

        private static readonly object _lck = new object();
        private static volatile AssemblyHandler _instance;
        private volatile IList<Type> _lstLoadedClasses;
        private volatile bool _bUpdateRequired;

        private AssemblyHandler()
        {
            _bUpdateRequired = true;
            _lstLoadedClasses = new ReadOnlyCollection<Type>(new List<Type>());
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            _bUpdateRequired = true;
        }

        private void UpdateAssemblies()
        {
            _bUpdateRequired = false;

            var classes = new ReadOnlyCollection<Type>((from ass in AppDomain.CurrentDomain.GetAssemblies() from type in GetLoadableTypes(ass) select type).ToList());
            _lstLoadedClasses = classes;
            try
            {
                if (ClassesLoaded != null)
                    ClassesLoaded(this, new ClassesLoadedEventArgs(classes));
            }
            catch (Exception ex)
            {

            }
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) return new Type[] { };//throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }

    public class ClassesLoadedEventArgs : EventArgs
    {
        public IList<Type> LoadedClasses { get; private set; }

        public ClassesLoadedEventArgs(IList<Type> loadedClasses)
        {
            LoadedClasses = loadedClasses;
        }
    }
}
