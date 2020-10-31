using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nugetversion.PackageReference
{
    public class PackageReferenceConsoleTableRenderer
    {
        const int startTabIdx = 30;
        
        public void RenderTable(PackageReferenceDic dic)
        {
            var maxNamePad = GetMaxNumPad(dic);
            foreach (var d in dic)
            {
                RenderProjectResults(startTabIdx, maxNamePad, d.Key,d.Value);
            }
        }

        public int GetMaxNumPad(PackageReferenceDic dic)
        {
            var maxNameWidth = dic.SelectMany(y => y.Value.Select(u => u.Name.Length))
                                  .OrderByDescending(x => x).First();
            var maxNamePad = maxNameWidth + 10;
            return maxNamePad;
        }

        public int GetMaxNumPad(IEnumerable<PackageReference> items)
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
                                        IEnumerable<PackageReference> items)
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
