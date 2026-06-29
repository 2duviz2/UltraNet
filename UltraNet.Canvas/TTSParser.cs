using System.Collections.Generic;

namespace UltraNet.Canvas;

public static class TTSParser
{
	public static Dictionary<string, string> parses = new Dictionary<string, string>
	{
		{ "<sprite=0>", "drool" },
		{ ":3", "meow" },
		{ "<", "less than" },
		{ ">", "greater than" }
	};

	public static string Parse(string text)
	{
		string t = text;
		foreach (KeyValuePair<string, string> p in parses)
		{
			t = t.Replace(p.Key, p.Value);
		}
		return t;
	}
}
