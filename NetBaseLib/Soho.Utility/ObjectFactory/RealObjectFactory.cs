using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Reflection;
using System.ComponentModel.Composition.Primitives;

namespace Soho.Utility
{
    internal class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;

        public SafeDirectoryCatalog(string directory)
        {
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    //ExceptionHelper.HandleException(ex);
                }
                catch (BadImageFormatException ex)
                {
                    //ExceptionHelper.HandleException(ex);
                }
            }
        }
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }
    }

    internal class RealObjectFactory<T> where T : class
    {
        public RealObjectFactory()
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string web_bin = Path.Combine(path, "bin");
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new SafeDirectoryCatalog(path));
            if (Directory.Exists(web_bin))
            {
                catalog.Catalogs.Add(new SafeDirectoryCatalog(web_bin));
            }

            if (TypeVersionConfig.Instance.AssemblyFolder != null && TypeVersionConfig.Instance.AssemblyFolder.Trim().Length > 0)
            {
                string[] folderList = TypeVersionConfig.Instance.AssemblyFolder.Trim().Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string d in folderList)
                {
                    if (d.Contains(":")) // 配置的是绝对路径
                    {
                        catalog.Catalogs.Add(new DirectoryCatalog(d));
                    }
                    else // 配置的是相对路径
                    {
                        catalog.Catalogs.Add(new DirectoryCatalog(Path.Combine(path, d)));
                    }
                }
            }
            CompositionContainer container = new CompositionContainer(catalog);

            container.ComposeParts(this);
        }

        [ImportMany]
        private IEnumerable<Lazy<T, IMetadata>> m_Operations = null;

        public T GetService(string version, string[] filters)
        {
            IEnumerable<Lazy<T, IMetadata>> list = new List<Lazy<T, IMetadata>>(m_Operations);
            foreach (Lazy<T, IMetadata> op in list)
            {
                if (op.Metadata.Version == version && CompareStringArray(op.Metadata.Filter, filters))
                {
                    return op.Value;
                }
            }
            return null;
        }

        private bool CompareStringArray(string[] arry1, string[] arry2)
        {
            bool arry1_is_empty = (arry1 == null || arry1.Length <= 0);
            bool arry2_is_empty = (arry2 == null || arry2.Length <= 0);
            if (arry1_is_empty && arry2_is_empty)
            {
                return true;
            }
            if ((arry1_is_empty && !arry2_is_empty)
                || (!arry1_is_empty && arry2_is_empty))
            {
                return false;
            }
            if (arry1.Length != arry2.Length)
            {
                return false;
            }
            for (int i = 0; i < arry1.Length; i++)
            {
                if (arry1[i] != arry2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public List<KeyValuePair<string, Type>> GetAllVersions()
        {
            List<Lazy<T, IMetadata>> list2 = new List<Lazy<T, IMetadata>>(m_Operations);
            List<KeyValuePair<string, Type>> list = new List<KeyValuePair<string, Type>>(list2.Count);
            foreach (Lazy<T, IMetadata> op in list2)
            {
                list.Add(new KeyValuePair<string, Type>(op.Metadata.Version, op.Value.GetType()));
            }
            return list;
        }
    }
}
