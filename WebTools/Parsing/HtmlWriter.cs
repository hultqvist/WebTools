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
		public readonly TextWriter Writer;

		/// <summary>
		/// Write parsing errors as HTML comments
		/// </summary>
		public bool WriteErrors { get; set; }

		public HtmlWriter()
		{
			this.Writer = new StringWriter();
		}

		public HtmlWriter(TextWriter writer)
		{
			this.Writer = writer;
		}

		public override string ToString()
		{
			return Writer.ToString();
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			tag.Attributes.Add(key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			Writer.Write("<" + tag.Name);
			foreach (var a in tag.Attributes)
				Writer.Write(" " + a.Key + "=\"" + HttpUtility.HtmlEncode(a.Value) + "\"");
			if (tag.SelfClosed)
				Writer.Write("/");
			Writer.Write(">");
		}

		public void ParsedClosingTag(Tag tag)
		{
			Writer.Write("</" + tag.Name + ">");
		}

		public void ParsedText(string decodedText)
		{
			Writer.Write(HttpUtility.HtmlEncode(decodedText));
		}

		public void ParseError(string message)
		{
			if (WriteErrors)
				Writer.Write("<!-- " + HttpUtility.HtmlEncode(message) + " -->");
		}
	}
}

