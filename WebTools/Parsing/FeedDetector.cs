using System;
using System.Collections.Generic;
using System.IO;

namespace SilentOrbit.Parsing
{
	/// <summary>
	/// Parse HTML and return all linked feeds 
	/// or if an Atom/RSS/RDF feeds then we have a single feed
	/// </summary>
	public class FeedDetector : TagParser
	{
		readonly List<FeedLink> list = new List<FeedLink>();
		DocType type = DocType.Undetermined;
		readonly FeedLink self = new FeedLink(null, null);

		public FeedDetector(Stream raw, string url) : base(raw)
		{
			self.Url = url;
		}

		public class FeedLink
		{
			public string Url { get; set; }

			public string Title { get; set; }

			public FeedLink(string url, string title)
			{
				this.Url = url;
				this.Title = title;
			}
		}

		enum DocType
		{
			Undetermined,
			Feed,
			FeedTitle,
			Html,
			StopParsing,
		}

		public List<FeedLink> Detect()
		{
			base.Parse();
			return list;
		}

		protected override void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			switch(key)
			{
				case "rel":
				case "type":
				case "title":
				case "href":
					tag.Attributes.Add(key, val);
					return;
			}
		}

		protected override void ParsedOpeningTag(Tag tag)
		{
			if (tag.Name.StartsWith("?"))
				return;
			if (type == DocType.Undetermined)
			{
				switch(tag.Name)
				{
					case "!doctype":
						return; //Ignore

					case "html":
						type = DocType.Html;
						return;
					case "feed":
					case "rss":
					case "rdf":
						type = DocType.Feed;
						list.Add(self);
						return;
					default:
						throw new NotImplementedException("Unknown root tag: " + tag.Name);
				}
			}
			if (type == DocType.Html)
			{
				//Scan all tags for feed links
				string href;
				if (tag.Attributes.TryGetValue("href", out href) == false)
					return;
				string attrType;
				if (tag.Attributes.TryGetValue("type", out attrType) == false)
					return;
				string title = href;
				tag.Attributes.TryGetValue("title", out title);

				switch(attrType)
				{
					case "application/atom+xml":
					case "application/rss+xml":
					case "application/rdf+xml":
						list.Add(new FeedLink(href, title));
						return;
				}
				return;
			}
			if (type == DocType.Feed)
			{
				//Scan for feed title
				if (tag.Name == "title")
				{
					type = DocType.FeedTitle;
					return;
				}

				//TODO: set listen state
				//TODO: next text parsed is the title.

				return;
			}

			throw new InvalidOperationException();
		}

		protected override void ParsedClosingTag(Tag tag)
		{
			//End feed title
			if (type == DocType.FeedTitle)
				type = DocType.Feed;
		}

		protected override void ParsedText(string decodedText)
		{
			//Feed title
			if (type == DocType.FeedTitle && self.Title == null)
				self.Title += decodedText;
		}
	}
}

