using System;

namespace nugetversion
{
    public static class ConsoleRender
    {
        public class ConsoleRenderObj { }

        public static ConsoleRenderObj W(string msg)
        {
            return new ConsoleRenderObj().W(msg);
        }

        public static ConsoleRenderObj W(string msg, ConsoleColor color)
        {
            return new ConsoleRenderObj().W(msg, color);
        }

        public static ConsoleRenderObj W(this ConsoleRenderObj render, string msg)
        {
            Console.Write(msg);
            return render;
        }

        public static ConsoleRenderObj W(this ConsoleRenderObj render, string msg, ConsoleColor color)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ForegroundColor = orig;
            return render;
        }
    }

}
