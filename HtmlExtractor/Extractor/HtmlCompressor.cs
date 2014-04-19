using System;
using System.Text.RegularExpressions;
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
			if (string.IsNullOrWhiteSpace(val))
				return;
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

		/// <summary>
		/// Trim all but space
		/// </summary>
		static Regex trim = new Regex(@"[\t\n\r]+");

		public void ParsedText(string decodedText)
		{
			string trimmed = trim.Replace(decodedText, "");
			output.ParsedText(trimmed);
		}

		public void ParseError(string message)
		{
			output.ParseError(message);
		}
	}
}

