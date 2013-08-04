using System;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Parses all tags in a file and return a "DOM" using Tag objects
	/// </summary>
	public class DocumentParser : TagParser
	{
		public DocumentParser(string s) : base(s)
		{
		}

		public Tag ParseDocument()
		{
			base.Parse();
			return document;
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

		protected override void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			if (tag.Attributes.ContainsKey(key) == false)
				tag.Attributes.Add(key, val);
			else if (tag.Attributes.ContainsKey(ns + ":" + key) == false)
				tag.Attributes.Add(ns + ":" + key, val);
		}

		protected override void ParsedOpeningTag(Tag tag)
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

		protected override void ParsedClosingTag(Tag tag)
		{
			previous = current;
			current = current.Parent;
		}

		protected override void ParsedText(string decodedText)
		{
			if (current == null)
				return; //Still at root

			if (previous != null)
			{
				if (previous.After == null)
					previous.After = decodedText;
				else
					throw new InvalidProgramException("Already got text after");
			}
			else
			{
				current.Value += decodedText;
			}
		}
	}
}

