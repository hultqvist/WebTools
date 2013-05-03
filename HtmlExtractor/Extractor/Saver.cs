using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SilentOrbit.Code;

namespace SilentOrbit.Extractor
{
	class Saver : SilentOrbit.ProtocolBuffers.CodeWriter
	{
		public Saver(string path, string rootNamespace) : base(path)
		{
			IndentPrefix = "\t";
			WriteLine("using SharpKit.JavaScript;");
			Bracket("namespace " + rootNamespace);
		}

		public override void Dispose()
		{
			EndBracket();
			base.Dispose();
		}

		public void WriteClass(string ns, HtmlData data)
		{
			if (ns == "Fragment")
			{
				Comment("Fragment");
				//WriteLine("[JsType(JsMode.Prototype)]");
				//WriteLine("[JsType(JsMode.Json)]");
				Bracket("public partial class " + data.ClassName);
			}
			else
			{
				if (ns != "")
				{
					Comment("Non null ns: " + ns);
					WriteLine("[JsType(JsMode.Prototype)]");
					Bracket("public static partial class " + ns);
				}
				else
					Comment("null ns");
				WriteLine("[JsType(JsMode.Json)]");
				Bracket("public static partial class " + data.ClassName);
			}
			WriteLine("public const string StateName = \"" + Path.GetFileNameWithoutExtension(data.FileName) + "\";");
			WriteLine("public const string FileName = \"" + data.FileName + "\";");

			Bracket("public static class CSS");
			foreach(var sub in data.Elements)
				WriteElements("", sub);
			EndBracket();

			EndBracket();
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
			Bracket("public static class " + Name.ToCamelCase(sel.Selector));

			if (sel.Type == SelectorType.ID)
				WriteLine("public const string ID = \"" + cssSelector + "\";");
			if (sel.Type == SelectorType.Class)
				WriteLine("public const string Class = \"" + cssSelector + "\";");

			/*Bracket("public override string ToString()");
			if (sel.Type == SelectorType.ID)
				WriteLine("return ID;");
			if (sel.Type == SelectorType.Class)
				WriteLine("return Class;");
			EndBracket();*/

			foreach(var sub in sel.Elements)
				WriteElements(cssSelector, sub);

			EndBracket();
		}

		public void WriteClasses(List<string> classes)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class Classes");

			foreach(string c in classes)
				WriteLine("public const string " + Name.ToCamelCase(c) + " = \"." + c + "\";");

			EndBracket();
		}
	}
}

