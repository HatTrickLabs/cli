using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace HatTrick.CommandLine
{
    #region resource accessor
    internal class TemplateAccessor
    {
        #region interface
        private Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();
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