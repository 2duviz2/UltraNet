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

    public static class TTSParser
    {
        public static Dictionary<string, string> parses = new()
        {
            { "<sprite=0>", "drool" },
            //{ "ik", "i know" },
            //{ "idk", "i dont know" },
            //{ "ts", "this shit" },
            //{ "tf", "the fuck" },
            //{ "ig", "i guess" },
            //{ "idc", "i dont care" },
            //{ "omw", "on my way" },
            //{ "btw", "by the way" },
            //{ "ngl", "not gonna lie" },
            //{ "atp", "at this point" },
            //{ "gl", "good luck" },
            //{ "gj", "good job" },
            //{ "nvm", "nevermind" },
            //{ "rn", "right now" },
            //{ "np", "no problem" },
            //{ "mf", "motherfucker" },
            //{ "wdym", "what do you mean" },
            { ":3", "meow" },
            //{ "<noparse>", "" },
            //{ "</noparse>", "" },
            { "<", "less than" },
            { ">", "greater than" },
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