using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Write parsed html as plain text
	/// </summary>
	public class PlainWriter : ITagOutput
	{
		/// <summary>
		/// Tag names allowed
		/// </summary>
		static readonly List<string> blockTags = new List<string>()
		{
			"article", "aside", 
			"blockquote", "br",
			"dd", "div", "dl", "dt",
			"footer",
			"h1", "h2", "h3", "h4", "h5", "h6", "header", "hr",
			"img", "li",
			"nav",
			"ol", "output",
			"p", "pre",
			"q",
			"rp", "rt", "ruby",
			"section", 
			"table", "tr",
			"ul",
		};
		
		enum TagVisibility
		{
			/// <summary>
			/// Render the tag text as text
			/// </summary>
			Escaped = 0,
			/// <summary>
			/// Render the tag as html
			/// </summary>
			Html = 1,
			/// <summary>
			/// Don't render the tag but do render its children
			/// </summary>
			HiddenTag = 2,
			/// <summary>
			/// Don't render tag or its children
			/// </summary>
			Hidden = 3,
		}

		readonly TextWriter writer;
		/// <summary>
		/// Top tag that is hidden,
		/// Nothing will be written until we reach the matching endtag
		/// </summary>
		Tag hidden = null;

		public PlainWriter(TextWriter writer)
		{
			this.writer = writer;
		}

		public static void Clean(string raw, TextWriter writer)
		{
			var c = new PlainWriter(writer);
			TagParser.Parse(raw, c);
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			key = key.ToLowerInvariant();
			val = HttpUtility.HtmlDecode(val);
			tag.Attributes.Add(key, val);
		}

		/// <summary>
		/// Possibly selfclosed
		/// </summary>
		public void ParsedOpeningTag(Tag tag)
		{
			string name = tag.Name;
			switch (name)
			{
				case "hr":
				case "br":
				case "img":
				case "?xml":
				case "!doctype":
					tag.SelfClosed = true;
					break;
			}

			if (hidden != null)
				return;

			if (HtmlCleaner.HiddenTags.Contains(name))
				return;
			else if (HtmlCleaner.HiddenTagsChildren.Contains(name))
			{
				hidden = tag;
				return;
			}


			if (blockTags.Contains(tag.Name) && tag.SelfClosed)
				writer.WriteLine();
			if (tag.Name == "a")
			{
				string src = tag.Attribute("src");
				if (src != null)
					writer.Write(" " + src + " ");
			}
			
			if (tag.Name == "img")
			{
				string url = tag.Attribute("src");
				string alt = tag.Attribute("alt");
				if (url == null)
					return;
					
				writer.Write("[" + alt + ": " + url + "]");
				return;
			}
		}

		public void ParsedClosingTag(Tag tag)
		{
			if (hidden != null)
			{
				//Closing hidden tag, time to show tags again
				if (hidden == tag)
					hidden = null;
				return;
			}

			string name = tag.Name;

			if(blockTags.Contains(name))
				writer.WriteLine();
		}

		public void ParsedText(string decodedText)
		{
			if (hidden != null)
				return;

			writer.Write(decodedText);
		}

		public void ParseError(string message)
		{
		}
	}
}

