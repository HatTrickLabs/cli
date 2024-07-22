using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HatTrick.CommandLine
{
    internal class VersionInquiryHandler
    {
        #region internals
        private List<(string name, Version version)> _assemblyVersionInfo;
        #endregion

        #region constructors
        internal VersionInquiryHandler()
        {
            _assemblyVersionInfo = new List<(string name, Version version)>();
        }
        #endregion

        #region go
        internal void Go(Command cmd)
        {
            Assembly assem = Assembly.GetEntryAssembly();
            this.BuildAssemblyVersionInfo(assem);
            int maxLen = _assemblyVersionInfo.Max(itm => itm.name.Length);
            string format = $"{{0,-{maxLen + 2}}} {{1}}";
            foreach (var v in _assemblyVersionInfo)
            {
                Console.WriteLine(format, v.name + ":", $"{v.version.Major}.{v.version.Minor}.{v.version.Build}");
            }
        }
        #endregion

        #region render hattrick assembly version info
        private void BuildAssemblyVersionInfo(Assembly assembly)
        {
            foreach (var name in assembly.GetReferencedAssemblies())
            {
                if (name.Name.StartsWith("system", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    if (!_assemblyVersionInfo.Exists((itm) => string.Compare(itm.name, name.Name, true) == 0))
                    {
                        this.BuildAssemblyVersionInfo(Assembly.Load(name));
                        _assemblyVersionInfo.Add((name.Name, name.Version));
                    }
                }
            }
        }
        #endregion
    }
}
