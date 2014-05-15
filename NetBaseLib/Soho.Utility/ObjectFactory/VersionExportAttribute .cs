using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Soho.Utility
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VersionExportAttribute : ExportAttribute, IMetadata
    {
        public VersionExportAttribute(Type type)
            : this(type, TypeVersionConfig.VERSION_DEFAULT)
        {

        }

        public VersionExportAttribute(Type type, string version)
            : this(type, version, null)
        {
            Version = version;
        }

        public VersionExportAttribute(Type type, string[] filter)
            : this(type, TypeVersionConfig.VERSION_DEFAULT, filter)
        {

        }

        public VersionExportAttribute(Type type, string version, string[] filter)
            : base(type)
        {
            Version = version;
            if (filter == null)
            {
                Filter = new string[0];
            }
            else
            {
                Filter = filter;
            }
        }

        public string Version
        {
            get;
            set;
        }

        public string[] Filter
        {
            get;
            set;
        }
    }

}
