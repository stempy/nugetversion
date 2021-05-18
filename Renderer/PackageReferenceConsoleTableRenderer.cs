using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetVersion.PackageReference
{
    public class PackageReferenceConsoleTableRenderer
    {
        const int StartTabIdx = 30;
        
        public void RenderTable(PackageReferenceDic dic)
        {
            var maxNamePad = GetMaxNumPad(dic);
            foreach (var d in dic)
            {
                RenderProjectResults(StartTabIdx, maxNamePad, d.Key,d.Value);
            }
        }

        public int GetMaxNumPad(PackageReferenceDic dic)
        {
            var maxNameWidth = dic.SelectMany(y => y.Value?.Select(u => u.Name.Length))
                                  .OrderByDescending(x => x).First();
            var maxNamePad = maxNameWidth + 10;
            return maxNamePad;
        }

        public int GetMaxNumPad(IEnumerable<PackageReferenceModel> items)
        {
            var maxNameWidth = items.Select(u => u.Name.Length)
                                  .OrderByDescending(x => x).First();
            var maxNamePad = maxNameWidth + 10;
            return maxNamePad;
        }

        public void RenderProjectResults(int startTabIdx, PackageReferenceDic dic)
        {
            if (dic.Any())
            {
                var maxNamePad = this.GetMaxNumPad(dic);
                foreach (var d in dic)
                {
                    RenderProjectResults(startTabIdx, maxNamePad, d.Key, d.Value);
                }
            }
        }

        public void RenderProjectResults(int startTabIdx,
                                        int maxNamePad,
                                        string projectFile,
                                        IEnumerable<PackageReferenceModel> items)
        {
            var fileName = Path.GetFileName(projectFile);
            ConsoleRender.W($"{fileName}\n");
            var tabIdx = startTabIdx;
            var tabStr = new string(' ', tabIdx);
            foreach (var pr in items)
            {
                ConsoleRender.W($"{tabStr}{pr.Name.PadRight(maxNamePad)}", ConsoleColor.DarkCyan)
                             .W($"{pr.Version}\n", ConsoleColor.DarkMagenta);
            }
            Console.WriteLine();
        }
    }

}
