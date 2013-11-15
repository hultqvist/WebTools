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

		public void WriteFragment(string ns, HtmlData data)
		{
			if (ns == "")
				Bracket("namespace " + options.Namespace);
			else
				Bracket("namespace " + options.Namespace + "." + ns);

			WriteLine("[JsType(JsMode.Json)]");
			Bracket((options.AccessInternal ? "internal" : "public") +" partial class " + data.FragmentName);
			WriteLine("public const string StateName = \"" + Path.GetFileNameWithoutExtension(data.FileName) + "\";");
			if(options.GenerateFilenameProperties)
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
			Bracket("public class " + sel.ClassName + " : " + sel.SkType);

			//ID/Class and Selector
			if(options.MinimizeNames)
				WriteLine("#if DEBUG");
			
			WriteLine("public const string Selector = \"" + cssSelector + "\";");
			RenderIdSelectors(sel, false);

			//WriteLine("[JsField(Name=\"querySelector(\\\"" + cssSelector + "\\\")\")]");
			//WriteLine("public string Query;");

			if (options.MinimizeNames)
			{
				WriteLine("#else");

				WriteLine("public const string Selector = \"" + cssSelectorObfuscated + "\";");
				RenderIdSelectors(sel, true);
			
				WriteLine("#endif");
			}

			//Sub element classes
			foreach (var sub in sel.Elements)
				WriteElements(cssSelector, cssSelectorObfuscated, sub);

			EndBracket();
		}

		void RenderIdSelectors(SelectorData sel, bool obfuscate)
		{
			//ID and Element
			if (sel.Type == SelectorType.ID)
			{
				string id = (obfuscate ? ob.ObfuscateID(sel.Selector) : sel.Selector);
				WriteLine("public const string ID = \"" + id + "\";");

				if (options.GenerateElementProperties)
				{
					WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + id + "\\\")\", NativeField = true, Global = true)]");
					WriteLine("public static " + sel.ClassName + " Element { get { return null; } }");
				}
			}
			//Class
			if (sel.Type == SelectorType.Class)
			{
				string classname = (obfuscate ? ob.ObfuscateClass(sel.Selector) : sel.Selector);
				WriteLine("public const string Class = \"" + classname + "\";");
			}

			//Sub element properties
			if (sel.Elements.Count > 0)
			{
				WriteLine("[JsProperty(Name=\"\", NativeField = true)]");
				WriteLine("public ElementsClass By { get; set; }");
				Bracket("public class ElementsClass");
				foreach (var sub in sel.Elements)
				{
					//Not easily accessible, might as well use Name.Element
					/*if (sub.Type == SelectorType.ID)
					{
						string id = (obfuscate ? ob.ObfuscateID(sub.Selector) : sub.Selector);
						WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + id + "\\\")\", Global = true, NativeField = true)]");
						WriteLine("public static " + sub.ClassName + " " + sub.PropertyName + " { get; set; }");
					}*/
					if (sub.Type == SelectorType.Class)
					{
						string classname = (obfuscate ? ob.ObfuscateClass(sub.Selector) : sub.Selector);
						WriteLine("[JsProperty(Name=\"getElementsByClassName(\\\"" + classname + "\\\")[0]\", NativeField = true)]");
						WriteLine("public " + sub.ClassName + " " + sub.PropertyName + " { get; set; }");
					}
				}
				EndBracket();
			}

		}

		public void WriteClasses(List<string> classes)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket((options.AccessInternal ? "internal" : "public") + " static class Classes");

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

