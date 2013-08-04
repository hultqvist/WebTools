using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SilentOrbit.Code;

namespace SilentOrbit.Extractor
{
	class Saver : CodeWriter
	{
		readonly Options options;

		public Saver(Options options) : base(options.OutputCS)
		{
			this.options = options;
			IndentPrefix = "\t";
			NewLine = "\n";
			WriteLine("using SharpKit.JavaScript;");
			WriteLine("using SharpKit.Html;");
		}

		public void WriteClass(string ns, HtmlData data)
		{
			if(ns == "")
				Bracket("namespace " + options.Namespace);
			else
				Bracket("namespace " + options.Namespace + "." + ns);

			Bracket("public partial class " + data.ClassName);
			WriteLine("public const string StateName = \"" + Path.GetFileNameWithoutExtension(data.FileName) + "\";");
			WriteLine("public const string FileName = \"" + data.FileName + "\";");

			foreach (var sub in data.Elements)
				WriteElements("", sub);

			EndBracket(); //class
			EndBracket(); //namespace
		}

		public void WriteElements(string selector, SelectorData sel)
		{
			string cssSelector;
			if (sel.Type == SelectorType.ID)
				cssSelector = "#" + sel.Selector;
			else if (sel.Type == SelectorType.Class)
				cssSelector = (selector + " ." + sel.Selector).Trim();
			else
				throw new NotImplementedException();

			/*if (sel.Elements.Count == 0)
			{
				WriteLine("public const string " + Name.ToCamelCase(sel.Selector) + " = \"" + cssSelector + "\";");
				return;
			}*/

			WriteLine("[JsType(JsMode.Json)]");
			string className = Name.ToCamelCase(sel.Selector);
			if (sel.Type == SelectorType.ID)
				className = "Id" + className;
			if (sel.Type == SelectorType.Class)
				className = "Class" + className;
			Bracket("public static class " + className);

			//ID/Class and Selector
			WriteLine("public const string Selector = \"" + cssSelector + "\";");
			if (sel.Type == SelectorType.ID)
			{
				WriteLine("public const string ID = \"" + sel.Selector + "\";");

				WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + sel.Selector + "\\\")\", NativeField=true, Global=true)]");
				WriteLine("public static HtmlElement Element { get { return null; } }");
			}
			if (sel.Type == SelectorType.Class)
				WriteLine("public const string Class = \"" + sel.Selector + "\";");

			//Sub elements
			foreach (var sub in sel.Elements)
				WriteElements(cssSelector, sub);

			EndBracket();
		}

		public void WriteClasses(List<string> classes)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class Classes");

			foreach (string c in classes)
				WriteLine("public const string " + Name.ToCamelCase(c) + " = \"." + c + "\";");

			EndBracket();
		}
	}
}

