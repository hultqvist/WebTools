using System;
using System.IO;
using SilentOrbit.Parsing;

namespace SilentOrbit.Extractor
{
	/// <summary>
	/// Compress and obfuscate
	/// </summary>
	public class HtmlCompressor : ITagOutput
	{
		readonly ITagOutput output;

		public HtmlCompressor(ITagOutput output)
		{
			this.output = output;
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			output.ParsedAttribute(tag, ns, key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			output.ParsedOpeningTag(tag);
		}

		public void ParsedClosingTag(Tag tag)
		{
			output.ParsedClosingTag(tag);
		}

		public void ParsedText(Tag tag, string decodedText)
		{
			output.ParsedText(tag, decodedText.Trim(' ', '\t', '\r', '\n'));
		}

		public void ParseError(string message)
		{
			output.ParseError(message);
		}
	}
}

