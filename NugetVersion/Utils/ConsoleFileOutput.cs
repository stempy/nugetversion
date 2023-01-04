using System;
using System.IO;

namespace NugetVersion.Utils
{
    internal static class ConsoleFileOutput
    {
        private static TextWriter StdOut = Console.Out;
        private static TextWriter StdErr = Console.Error;

        public static void RedirectConsoleToFile(string file)
        {
            FileStream filestream = new FileStream(file, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;

            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
        }

        public static void EndRedirection()
        {
            Console.SetOut(StdOut);
            Console.SetError(StdErr);
        }
    }
}