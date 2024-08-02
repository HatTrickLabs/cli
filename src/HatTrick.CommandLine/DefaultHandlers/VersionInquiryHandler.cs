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
            int padTo = maxLen + 3;

            foreach (var v in _assemblyVersionInfo)
            {
                string pad = new string('.', padTo - v.name.Length);
                Version ver = v.version;
                string line = string.Concat(v.name, pad, ver.Major, ".", ver.Minor, ".", ver.Build);
                Console.WriteLine(line);
            }
        }
        #endregion

        #region build assembly version info
        private void BuildAssemblyVersionInfo(Assembly assembly)
        {
            var assemName = assembly.GetName();
            _assemblyVersionInfo.Add((assemName.Name, assemName.Version));
            foreach (var name in assembly.GetReferencedAssemblies())
            {
                if (name.Name.StartsWith("system", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!_assemblyVersionInfo.Exists((itm) => string.Compare(itm.name, name.Name, true) == 0))
                {
                    this.BuildAssemblyVersionInfo(Assembly.Load(name));
                    if (!_assemblyVersionInfo.Exists((itm) => string.Compare(itm.name, name.Name, true) == 0))
                        _assemblyVersionInfo.Add((name.Name, name.Version));
                }
            }
        }
        #endregion
    }
}
