using System;
using System.Collections.Generic;

namespace SilentOrbit
{
	public class Obfuscator
	{
		readonly Dictionary<string,string> obfuscated = new Dictionary<string, string>();
		readonly Dictionary<string,string> deobfuscated = new Dictionary<string, string>();

		public Obfuscator()
		{
		}

		static string valid = "abcdefghijklmnopqrstuvwxyz";

		string GetObfuscated(string original)
		{
			if (obfuscated.ContainsKey(original))
				return obfuscated[original];

			//Generate obfuscated id
			int c = obfuscated.Count;
			string ob = "";
			while (true)
			{
				ob += valid[c % valid.Length];
				c /= valid.Length;
				if (c <= 0)
					break;
			}

			obfuscated.Add(original, ob);
			deobfuscated.Add(ob, original);
			return ob;
		}

		public string ObfuscateID(string id)
		{
			return GetObfuscated(id);
		}

		public string ObfuscateClass(string className)
		{
			return GetObfuscated(className);
		}
	}
}

