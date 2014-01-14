using System;
using System.Collections.Generic;
using System.IO;
using SilentOrbit.Code;
using SilentOrbit.Parsing;
using SilentOrbit.Data;

namespace SilentOrbit.Extractor
{
	public class HtmlClassIdExtractor: ITagOutput
	{
		readonly ITagOutput output;
		public readonly HtmlData HtmlData = new HtmlData();
		readonly List<StackItem> stack = new List<StackItem>();

		class StackItem
		{
			public Tag Tag { get; set; }
			public readonly List<SelectorData> Selectors = new List<SelectorData>();

			public StackItem(Tag t)
			{
				this.Tag = t;
			}
		}

		public HtmlClassIdExtractor(ITagOutput output, string path)
		{
			this.output = output;

			HtmlData.FileName = Path.GetFileName(path);
			HtmlData.FragmentName = Name.ToCamelCase(Path.GetFileNameWithoutExtension(path));

			var s = new StackItem(null);
			s.Selectors.Add(HtmlData);
			stack.Add(s);
		}

		public void ParsedAttribute(Tag tag, TagNamespace ns, string key, string val)
		{
			if(tag.Attributes.ContainsKey(key))
				throw new ArgumentException("duplicate key: " + key + " = " + val);
			tag.Attributes.Add(key, val);
		}

		public void ParsedOpeningTag(Tag tag)
		{
			//Get them before they get obfuscated
			string id = tag.Attribute("id");
			var cs = tag.Attribute("class");

			//Ofuscation being done here
			output.ParsedOpeningTag(tag);

			string[] classes = cs == null ? new string[0] : cs.Split(' ');

			if (id == null && classes.Length == 0)
				return;

			var s = new StackItem(tag);

			foreach (var data in stack[stack.Count - 1].Selectors)
			{
				//Extract local ID
				if (id != null)
					s.Selectors.Add(data.CreateID(id, tag.Name));

				//Extract local classes
				foreach (string c in classes)
				{
					if (c == "")
						continue;

					s.Selectors.Add(data.CreateClass(c, tag.Name));
				}
			}

			if (tag.SelfClosed)
				return;

			//Add to stack
			if(s.Selectors.Count > 0)
				stack.Add(s);
		}

		public void ParsedClosingTag(Tag tag)
		{
			output.ParsedClosingTag(tag);

			var top = stack[stack.Count - 1];
			if (top.Tag == tag)
				stack.Remove(top);
		}

		public void ParsedText(Tag tag, string decodedText)
		{
			output.ParsedText(tag, decodedText);
		}

		public void ParseError(string message)
		{
			output.ParseError(message);
		}
	}
}

