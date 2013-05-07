using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using SilentOrbit.Code;

namespace SilentOrbit.Extractor
{
	public class HtmlParser
	{
		readonly XDocument doc;
		readonly string path;

		HtmlParser(string path)
		{
			this.path = path;

			string text = File.ReadAllText(path, Encoding.UTF8);
			if (text.StartsWith("<!DOCTYPE", StringComparison.InvariantCulture) == false)
				text = "<div>" + text + "</div>";
			text = text.Replace("&nbsp;", " ");
			XmlReaderSettings xrs = new XmlReaderSettings();
			xrs.DtdProcessing = DtdProcessing.Parse;
			xrs.CheckCharacters = true;
			xrs.ConformanceLevel = ConformanceLevel.Auto;
			var xr = XmlReader.Create(new StringReader(text), xrs);

			doc = XDocument.Load(xr);
		}

		public static HtmlData Extract(string path)
		{
			var parser = new HtmlParser(path);
			return parser.ExtractData();
		}

		HtmlData ExtractData()
		{
			HtmlData data = new HtmlData();
			data.FileName = Path.GetFileName(path);
			data.ClassName = Name.ToCamelCase(Path.GetFileNameWithoutExtension(path));
			Extract(data, doc.Root);
			return data;
		}

		static void Extract(SelectorData data, XElement element)
		{
			var list = new List<SelectorData>();
			//Extract local ID
			var IDattr = element.Attribute("id");
			if (IDattr != null)
			{
				list.Add(data.CreateID(IDattr.Value));
			}

			//Extract local classes
			var attrClasses = element.Attribute("class");
			if (attrClasses != null)
			{
				var classes = attrClasses.Value.Split(' ');
				foreach (string c in classes)
				{
					if (c == "")
						continue;

					list.Add(data.CreateClass(c));
				}
			}

			//Get subelements
			foreach (var xe in element.Elements())
			{
				if (list.Count == 0)
				{
					Extract(data, xe);
				}
				else
				{
					foreach (var l in list)
					{
						Extract(l, xe);
					}
				}
			}
		}
	}
}

