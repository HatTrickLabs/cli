using System;

namespace HatTrick.CommandLine
{
    internal class VersionInquiryHandler
    {
        #region constructors
        internal VersionInquiryHandler()
        {
        }
        #endregion

        #region go
        internal void Go(Command cmd)
        {
            Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"{v.Major}.{v.Minor}.{v.Build}");
        }
        #endregion
    }
}
