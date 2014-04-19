using System;
using System.IO;
using System.Web;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Write the parsed tags directly to a TextWriter
	/// </summary>
	public class HtmlWriter : ITagOutput
	{
		readonly TextWriter writer;

		public HtmlWriter(TextWriter writer)
		{
			this.writer = writer;
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			//Nothing done here, rendered in ParsedOpeningTag
		}

		public void ParsedOpeningTag(Tag tag)
		{
			writer.Write("<" + tag.Name);
			foreach (var a in tag.Attributes)
				writer.Write(" " + a.Key + "=\"" + HttpUtility.HtmlEncode(a.Value) + "\"");
			if (tag.SelfClosed)
				writer.Write("/");
			writer.Write(">");
		}

		public void ParsedClosingTag(Tag tag)
		{
			writer.Write("</" + tag.Name + ">");
		}

		public void ParsedText(string decodedText)
		{
			writer.Write(HttpUtility.HtmlEncode(decodedText));
		}

		public void ParseError(string message)
		{
			writer.Write("<!-- " + HttpUtility.HtmlEncode(message) + " -->");
		}
	}
}

