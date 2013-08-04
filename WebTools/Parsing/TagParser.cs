using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Parse any xml like document.
	/// </summary>
	public abstract class TagParser
	{
		static char[] wsp = new char[] { ' ', '\t', '\r', '\n' };
		static char[] tagWsp = new char[] { ' ', '\t', '\r', '\n', '/', '>' };
		readonly string raw;
		Tag topTag = null;

		protected TagParser(Stream stream)
		{
			using(TextReader r = new StreamReader(stream, Encoding.UTF8))
				this.raw = r.ReadToEnd();
		}

		protected TagParser(string raw)
		{
			this.raw = raw;
		}
		//Idea needs more work: Remake as iterator
		/*
			 * var parser;
			 * foreach(var tag in parser.Root)
			 * {
			 * 	//each iteration will skip all children of the current tag
			 * 
			 * 		//unless we grab it and ask for its children
			 * 		//detect tag....
			 * 		if(tag.name = "entry")
			 * 		{
			 * 			foreach(
			 * 		}
			 * }
			 * 
			 * Special for html scanner which will ignore hiearchy
			 * Issue: what to iterate, tags, tags+text, nested or flat
			 */
		int parsed = 0;
		int tagStart = 0;
		int pos = 0;
		int tagEnd = 0;

		protected void Parse()
		{
			while (true)
			{
				//Find tag start
				tagStart = raw.IndexOf('<', pos);
				if (tagStart < 0)
					break;//remaining is text
				pos = tagStart + 1;

				//Special tags
				if (raw[pos] == '!')
				{
					WriteUnparsed(tagStart);

					//<!--Comments
					if (raw.Substring(pos, 3) == "!--")
					{
						int commentEnd = raw.IndexOf("-->", pos);
						if (commentEnd < 0)
							break;

						pos = commentEnd + 3;
						parsed = pos;
						continue;
					}

					// <![DCATA[
					if (raw.Substring(pos, 8) == "![CDATA[")
					{
						pos += 8;
						int cdataEnd = raw.IndexOf("]]>", pos);
						string cdata;
						if (cdataEnd < 0)
						{
							//Remaining is CDATA, though an error, this will be our iterpretation
							cdata = raw.Substring(pos);
							ParsedText(cdata);
							parsed = raw.Length;
							break;
						}
						cdata = raw.Substring(pos, cdataEnd - pos);
						ParsedText(cdata);

						pos = cdataEnd + 3;
						parsed = pos;
						continue;
					}
				}

				//Find end of tag
				tagEnd = raw.IndexOfAny(new char[] { '>', '<' }, pos);
				if (tagEnd < 0)
					break;
				if (raw[tagEnd] == '<')
				{
					//Invalid tag, treat as text
					pos = tagEnd;
					continue;
				}

				//Determine if closing tag
				bool closing = raw[pos] == '/';
				if (closing)
					pos += 1;

				//Got a tag from pos to end
				int nameEnd = raw.IndexOfAny(tagWsp, pos);
				if (nameEnd < 0)
					break;
				if (nameEnd > tagEnd)
				{
					pos = tagEnd + 1;
					continue;
				}

				string name = raw.Substring(pos, nameEnd - pos).ToLowerInvariant();
				pos = tagEnd + 1;

				//Namespace is added later below
				var tag = new Tag(null, name, topTag);

				//Parse attributes
				ParseAttributes(tag, nameEnd, tagEnd);

				//Namespace usage, parse after attributes in case a new xmlns was introduced there
				tag.Namespace = ParseNamespacePrefix(tag, ref name);
				tag.Name = name;

				//Send text before tag
				WriteUnparsed(tagStart);

				//Write tag
				if (closing)
					GotClosingTag(tag);
				else
					GotOpeningTag(tag);
			}

			//Escape remaining text in raw
			WriteUnparsed(raw.Length);

			//Close remaining tags
			while (topTag != null)
			{
				GotClosingTag(topTag);
			}
		}

		TagNamespace ParseNamespacePrefix(Tag root, ref string name)
		{
			int nsSep = name.IndexOf(':');
			if (nsSep < 0)
			{
				if (root.NS.ContainsKey(""))
					return root.NS[""];
				else
					return null;
			}

			string nsKey = name.Substring(0, nsSep);
			name = name.Substring(nsSep + 1);
			if (root.NS.ContainsKey(nsKey))
				return root.NS[nsKey];

			//Hardcoded namespaces
			if (nsKey == "xml")
				return nsKey;

			//else use the prefix as namespace
			ParseError("Missing namespace: " + nsKey + ":" + name);
			return nsKey;
		}

		protected abstract void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val);

		void ParseAttributes(Tag tag, int start, int end)
		{
			for (int p = start; p < end; p++)
			{
				char c = raw[p];
				//Skip spaces
				if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
					continue;

				if (c == '/')
				{
					tag.SelfClosed = true;
					continue;
				}

				//Start of attribute name
				int eqP = raw.IndexOf('=', p);
				if (eqP < 0 || eqP >= end)
					break;

				string key = raw.Substring(p, eqP - p).Trim(wsp).ToLowerInvariant();

				int quotStart = raw.IndexOfAny(new char[] { '"', '\'' }, eqP);
				if (quotStart < 0 || quotStart >= end)
					break;
				int quotEnd = raw.IndexOf(raw[quotStart], quotStart + 1);
				if (quotEnd < 0 || quotEnd >= end)
					break;
				p = quotEnd;

				string val = raw.Substring(quotStart + 1, quotEnd - quotStart - 1);
				val = HttpUtility.HtmlDecode(val);

				//Namespace declarations
				if (key == "xmlns")
				{
					//Need to create a new ns
					if (tag.Parent != null && tag.Parent.NS == tag.NS)
						tag.NS = new Dictionary<string, TagNamespace>(tag.Parent.NS);
					if (tag.NS.ContainsKey(""))
						tag.NS[""] = val;
					else
						tag.NS.Add("", val);
					continue;
				}
				if (key.StartsWith("xmlns:"))
				{
					//Need to create a new ns
					if (tag.Parent != null && tag.Parent.NS == tag.NS)
						tag.NS = new Dictionary<string, TagNamespace>(tag.Parent.NS);

					key = key.Substring(6);
					if (tag.NS.ContainsKey(key))
						tag.NS[key] = val;
					else
						tag.NS.Add(key, val);
					continue;
				}

				//Namespace usage
				var nsAttr = ParseNamespacePrefix(tag, ref key);
				ParsedAttribute(tag, nsAttr, key, val);
			}
		}

		protected abstract void ParsedOpeningTag(Tag tag);

		void GotOpeningTag(Tag tag)
		{
			parsed = pos;

			if (!tag.SelfClosed)
				topTag = tag;

			ParsedOpeningTag(tag);
		}

		protected abstract void ParsedClosingTag(Tag tag);

		void GotClosingTag(Tag tag)
		{
			//Scan from top of stack
			if (topTag == null || !topTag.HasMatchingStartTag(tag))
				return; //More to skip

			//Close all tags up to the closing tag
			while (topTag.Name != tag.Name && topTag.Namespace != tag.Namespace)
			{
				ParseError("Missing matching close tag for <" + topTag.Name + ">");
				ParsedClosingTag(topTag);
				topTag = topTag.Parent;
			}
			ParsedClosingTag(topTag);
			topTag = topTag.Parent;

			parsed = pos;
		}

		protected abstract void ParsedText(string decodedText);

		void WriteUnparsed(int toPos)
		{
			//Preserve some already encoded text
			if (parsed < toPos)
			{
				string decoded = HttpUtility.HtmlDecode(raw.Substring(parsed, toPos - parsed));
				ParsedText(decoded);
			}

			parsed = toPos;
		}

		protected virtual void ParseError(string message)
		{
			#if DEBUG
			Console.WriteLine(message);
			#endif
		}
	}
}

