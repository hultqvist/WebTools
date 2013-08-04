using System;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Read a feed and parse containing 
	/// </summary>
	public class FeedParser : TagParser
	{
		public FeedParser(string raw) : base(raw)
		{
		}

		protected override void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			throw new NotImplementedException();
		}

		protected override void ParsedOpeningTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		protected override void ParsedClosingTag(Tag tag)
		{
			throw new NotImplementedException();
		}

		protected override void ParsedText(string decodedText)
		{
			throw new NotImplementedException();
		}
	}
}

