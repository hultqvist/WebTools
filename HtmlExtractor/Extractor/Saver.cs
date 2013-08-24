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
		readonly Obfuscator ob;

		public Saver(Options options, Obfuscator ob) : base(options.OutputCS)
		{
			this.options = options;
			this.ob = ob;

			IndentPrefix = "\t";
			NewLine = "\n";
			WriteLine("using SharpKit.JavaScript;");
			WriteLine("using SharpKit.Html;");
		}

		public void WriteClass(string ns, HtmlData data)
		{
			if (ns == "")
				Bracket("namespace " + options.Namespace);
			else
				Bracket("namespace " + options.Namespace + "." + ns);

			Bracket("public partial class " + data.ClassName);
			WriteLine("public const string StateName = \"" + Path.GetFileNameWithoutExtension(data.FileName) + "\";");
			WriteLine("public const string FileName = \"" + data.FileName + "\";");

			foreach (var sub in data.Elements)
				WriteElements("", "", sub);

			EndBracket(); //class
			EndBracket(); //namespace
		}

		public void WriteElements(string selector, string obSelector, SelectorData sel)
		{
			string cssSelector;
			string cssSelectorObfuscated;
			if (sel.Type == SelectorType.ID)
			{
				cssSelector = "#" + sel.Selector;
				cssSelectorObfuscated = "#" + ob.ObfuscateID(sel.Selector);
			}
			else if (sel.Type == SelectorType.Class)
			{
				cssSelector = (selector + " ." + sel.Selector).Trim();
				cssSelectorObfuscated = (obSelector + " ." + ob.ObfuscateClass(sel.Selector)).Trim();
			}
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
			WriteLine("#if DEBUG");
			
			WriteLine("public const string Selector = \"" + cssSelector + "\";");
			RenderIdSelectors(sel, false);
			
			WriteLine("#else");

			WriteLine("public const string Selector = \"" + cssSelectorObfuscated + "\";");
			RenderIdSelectors(sel, true);
			
			WriteLine("#endif");

			//Sub elements
			foreach (var sub in sel.Elements)
				WriteElements(cssSelector, cssSelectorObfuscated, sub);

			EndBracket();
		}

		void RenderIdSelectors(SelectorData sel, bool obfuscate)
		{
			if (sel.Type == SelectorType.ID)
			{
				string id = (obfuscate ? ob.ObfuscateID(sel.Selector) : sel.Selector);
				WriteLine("public const string ID = \"" + id + "\";");
				
				string type = "HtmlElement";
				if (sel.TagName == "input")
					type = "HtmlInputElement";
				if (sel.TagName == "form")
					type = "HtmlFormElement";
				if (sel.TagName == "select")
					type = "HtmlSelectElement";
				
				WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + id + "\\\")\", NativeField=true, Global=true)]");
				WriteLine("public static " + type + " Element { get { return null; } }");
			}
			if (sel.Type == SelectorType.Class)
			{
				string classname = (obfuscate ? ob.ObfuscateClass(sel.Selector) : sel.Selector);
				WriteLine("public const string Class = \"" + classname + "\";");
			}
		}

		public void WriteClasses(List<string> classes)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class Classes");

			foreach (string c in classes)
			{
				string cc = Name.ToCamelCase(c);
				string co = ob.ObfuscateClass(c);

				WriteLine("#if DEBUG");
				WriteLine("public const string " + cc + "Selector = \"." + c + "\";");
				WriteLine("public const string " + cc + "Class = \"" + c + "\";");
				WriteLine("#else");
				WriteLine("public const string " + cc + "Selector = \"." + co + "\";");
				WriteLine("public const string " + cc + "Class = \"" + co + "\";");
				WriteLine("#endif");
			}

			EndBracket();
		}
	}
}

