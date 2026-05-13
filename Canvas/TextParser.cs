using System.Collections.Generic;

namespace UltraNet.Canvas
{
    public static class TextParser
    {
        public static Dictionary<string, string> parses = new()
        {
            { "//10", "/" },
            { "//11", "\\" },
            { "//12", ":" },
            { "//13", ";" },
            { "//14", "<noparse><</noparse>" },
            { "//15", "<noparse>></noparse>" },
            { "//1", "'" },
            { "//2", "\"" },
            { "//3", "(" },
            { "//4", ")" },
            { "//5", "$" },
            { "//6", "%" },
            { "//7", "@" },
            { "//8", "!" },
            { "//9", "#" },
            { ":drool:", "<sprite=0>" },
        };

        public static string Parse(string text)
        {
            string t = text;
            foreach (var p in parses)
                t = t.Replace(p.Key, p.Value);
            return t;
        }
    }
}