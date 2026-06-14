// SPDX-License-Identifier: Apache-2.0
// Copyright (c) HatTrick Labs, LLC

using System;
using System.IO;
using System.Reflection;

namespace HatTrick.CommandLine
{
    #region resource accessor
    internal class TemplateAccessor
    {
        #region internals
        private Assembly _assembly;
        #endregion

        #region interface
        private Assembly ExecutingAssembly => _assembly is null ? _assembly = Assembly.GetExecutingAssembly() : _assembly;
        #endregion

        #region get template
        internal string GetTemplate(string shortName)
        {
            string[] names = this.ExecutingAssembly.GetManifestResourceNames();
            string name = Array.Find(names, (n) => n.Contains(shortName, StringComparison.Ordinal));

            if (name is null)
                throw new ArgumentException($"No resource found containing provided short name: {shortName}", nameof(shortName));

            string value = GetResource(name);
            return value;
        }
        #endregion

        #region get resource
        private string GetResource(string fullName)
        {
            Assembly assem = this.ExecutingAssembly;

            string output = null;
            using (Stream stream = assem.GetManifestResourceStream(fullName))
            {
                using (StreamReader reader = new(stream))
                {
                    output = reader.ReadToEnd();
                }
            }
            return output;
        }
        #endregion
    }
    #endregion
}