using System;
using SilentOrbit.Parsing;
using SilentOrbit.Data;

namespace SilentOrbit.Extractor
{
	/// <summary>
	/// Obfuscates id and class attributes
	/// </summary>
	public class HtmlObfuscator : ITagOutput
	{
		readonly ITagOutput output;
		readonly Obfuscator ob;

		public HtmlObfuscator(Obfuscator ob, ITagOutput output)
		{
			this.ob = ob;
			this.output = output;
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			output.ParsedAttribute(tag, ns, key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			string id = tag.Attribute("id");
			if (id != null)
				tag.Attributes["id"] = ob.ObfuscateID(id);
			string classes = tag.Attribute("class");
			if (classes != null)
			{
				var cs = classes.Split(' ');
				for (int n = 0; n < cs.Length; n++)
					cs[n] = ob.ObfuscateClass(cs[n]);
				tag.Attributes["class"] = string.Join(" ", cs);
			}

			output.ParsedOpeningTag(tag);
		}

		public void ParsedClosingTag(Tag tag)
		{
			output.ParsedClosingTag(tag);
		}

		public void ParsedText(Tag tag, string decodedText)
		{
			output.ParsedText(tag, decodedText);
		}

		public void ParseError(string message)
		{
			output.ParseError(message);
		}
	}
}

