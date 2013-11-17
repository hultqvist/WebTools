using System;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit
{
	public class Obfuscator
	{
		readonly Dictionary<string,string> obfuscatedID = new Dictionary<string, string>();
		readonly Dictionary<string,string> obfuscatedClass = new Dictionary<string, string>();
		readonly Dictionary<string,string> deobfuscatedID = new Dictionary<string, string>();
		readonly Dictionary<string,string> deobfuscatedClass = new Dictionary<string, string>();

		public Obfuscator()
		{
		}

		public string Export()
		{
			StringBuilder s = new StringBuilder();
			foreach (var kvp in obfuscatedID)
			{
				s.AppendLine(kvp.Value + "\t#" + kvp.Key);
			}
			foreach (var kvp in obfuscatedClass)
			{
				s.AppendLine(kvp.Value + "\t." + kvp.Key);
			}
			return s.ToString();
		}

		/// <summary>
		/// Fill up the list with previously obfuscated classes.
		/// </summary>
		public void ExportClasses(List<string> classes)
		{
			foreach (var kvp in obfuscatedClass)
			{
				if(classes.Contains(kvp.Key))
				   continue;
				classes.Add(kvp.Key);
			}
		}

		static string valid = "abcdefghijklmnopqrstuvwxyz";

		string GetObfuscated(Dictionary<string,string> obd, Dictionary<string,string> deob, string original)
		{
			if (obd.ContainsKey(original))
				return obd[original];
			if (original == "")
				return original;

			//Generate obfuscated id
			int c = obd.Count;
			string ob = "";
			while (true)
			{
				ob += valid[c % valid.Length];
				c /= valid.Length;
				if (c <= 0)
					break;
			}

			obd.Add(original, ob);
			deob.Add(ob, original);
			return ob;
		}

		/// <summary>
		/// Return null if not already in the list
		/// </summary>
		public string GetObfuscateID(string id)
		{
			if (obfuscatedID.ContainsKey(id))
				return obfuscatedID[id];
			return null;
		}

		/// <summary>
		/// Return null if not already in the list
		/// </summary>
		public string GetObfuscateClass(string className)
		{
			if (obfuscatedClass.ContainsKey(className))
				return obfuscatedClass[className];
			return null;
		}

		public string ObfuscateID(string id)
		{
			return GetObfuscated(obfuscatedID, deobfuscatedID, id);
		}

		public string ObfuscateClass(string className)
		{
			return GetObfuscated(obfuscatedClass, deobfuscatedClass, className);
		}
	}
}

