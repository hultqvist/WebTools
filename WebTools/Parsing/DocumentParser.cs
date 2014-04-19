using System;
using System.Diagnostics;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Parses all tags in a file and return a "DOM" using Tag objects
	/// </summary>
	public class DocumentParser : ITagOutput
	{
		DocumentParser()
		{
		}

		public static Tag Parse(string source)
		{
			var p = new DocumentParser();
			TagParser.Parse(source, p);
			return p.document;
		}

		Tag document;
		/// <summary>
		/// any new tags are added as children to this one
		/// </summary>
		Tag current;
		/// <summary>
		/// Last closed tag, text is added as After to this
		/// </summary>
		Tag previous;

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			if (tag.Attributes.ContainsKey(key) == false)
				tag.Attributes.Add(key, val);
			else if (tag.Attributes.ContainsKey(ns + ":" + key) == false)
				tag.Attributes.Add(ns + ":" + key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			//Ignore tags before root
			if (current == null)
			{
				if (tag.SelfClosed)
				{
					if (tag.Name.StartsWith("?xml"))
						return;
					ParseError("Ignoring top level selfclosed: " + tag);
					return;
				}

				if (document != null)
				{
					ParseError("Ignoring top level after first root node: " + tag);
					return;
				}
				document = tag;
				current = tag;
				return;
			}

			current.Children.Add(tag);
			if (tag.SelfClosed)
			{
				previous = tag;
				return;
			}
			previous = null;
			current = tag;
		}

		public void ParsedClosingTag(Tag tag)
		{
			//Value is only null for selfclosed tags
			if (current.Value == null)
				current.Value = "";

			previous = current;
			current = current.Parent;
		}

		public void ParsedText(string decodedText)
		{
			if (current == null)
				return; //Still at root

			if (previous != null)
			{
				//Usually .After is only set once, then there is a new tag, but comments will break and cause this to be called twice once for each part.
				previous.After += decodedText;
			}
			else
			{
				current.Value += decodedText;
			}
		}

		public void ParseError(string message)
		{
			Debug.WriteLine("Parse ERROR: " + message);
		}
	}
}

