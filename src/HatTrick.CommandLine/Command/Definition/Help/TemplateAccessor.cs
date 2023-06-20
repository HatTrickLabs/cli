using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace HatTrick.CommandLine
{
    #region resource accessor
    public class TemplateAccessor
    {
        #region interface
        private Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();
        #endregion

        #region get template
        public string GetTemplate(string shortName)
        {
            //string fullName = $"{typeof(TemplateAccessor).Namespace}.Command.Definition.telp.templates.{shortName}.htt";
            //string value = this.GetResource(fullName);
            //return value;

            string[] names = this.ExecutingAssembly.GetManifestResourceNames();
            string name = Array.Find(names, (n) => n.Contains(shortName, StringComparison.OrdinalIgnoreCase));

            if (name is null)
                throw new ArgumentException($"No resource found containing provided short name: {shortName}", nameof(shortName));

            string value = this.GetResource(name);
            return value;
        }
        #endregion

        #region get
        private string GetResource(string fullName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();

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