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
			Bracket((options.AccessInternal ? "internal" : "public") + " partial class " + data.FragmentName);
			//WriteLine("public const string StateName = \"" + Path.GetFileNameWithoutExtension(data.FileName) + "\";");
			if (options.GenerateFilenameProperties)
				WriteLine("public const string FileName = \"" + data.FileName + "\";");

			WriteElements("", "", data);

			EndBracket(); //class
			EndBracket(); //namespace
		}

		public void WriteElements(string cssSelector, string cssSelectorObfuscated, SelectorData sel)
		{
			Console.WriteLine("WriteElements: " + sel);
			if (sel.SubID.Count + sel.SubClass.Count == 0)
			{
				WriteLine("//Empty " + sel.Type + " " + sel.Selector);
				return;
			}

			//CSS selectors
			if (sel.Type == SelectorType.ID)
			{
				cssSelector = "#" + sel.Selector;
				cssSelectorObfuscated = "#" + ob.ObfuscateID(sel.Selector);
			}
			if (sel.Type == SelectorType.Class)
			{
				cssSelector = (cssSelector + " ." + sel.Selector).Trim();
				cssSelectorObfuscated = (cssSelectorObfuscated + " ." + ob.ObfuscateClass(sel.Selector)).Trim();
			}

			//ID
			RenderSubID(sel);
			RenderSubElement(sel);
			//Class
			RenderSubClass(sel);

			//Selector
			RenderSubSelector(sel, cssSelector, cssSelectorObfuscated);

			if (options.MinimizeNames)
				WriteLine("#if DEBUG");
			RenderIdSelectors(sel, false);
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				RenderIdSelectors(sel, true);
				WriteLine("#endif");
			}

			//Sub element classes
			foreach (var sub in sel.SubID)
			{
				if (sub.SubID.Count + sub.SubClass.Count == 0)
					continue;

				WriteLine("[JsType(JsMode.Json)]");
				Bracket("public class " + sub.ClassName + " : " + SharpKitClasses.FromSelectorData(sub));
				WriteElements(cssSelector, cssSelectorObfuscated, sub);
				EndBracket();
			}
			foreach (var sub in sel.SubClass)
			{
				if (sub.SubID.Count + sub.SubClass.Count == 0)
					continue;

				WriteLine("[JsType(JsMode.Json)]");
				Bracket("public class " + sub.ClassName + " : " + SharpKitClasses.FromSelectorData(sub));
				WriteElements(cssSelector, cssSelectorObfuscated, sub);
				EndBracket();
			}
		}

		void RenderSubID(SelectorData sel)
		{
			if (sel.SubID.Count == 0)
				return;

			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class ID");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");
			foreach (var i in sel.SubID)
				WriteLine("public const string " + i.PropertyName + " = \"" + i.Selector + "\";");
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				foreach (var i in sel.SubID)
					WriteLine("public const string " + i.PropertyName + " = \"" + ob.ObfuscateID(i.Selector) + "\";");
				WriteLine("#endif");
			}
			EndBracket();
		}

		void RenderSubElement(SelectorData sel)
		{
			if (!options.GenerateElementProperties)
				return;
			if (sel.SubID.Count == 0)
				return;

			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class ElementById");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");
			foreach (var i in sel.SubID)
			{
				WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + i.Selector + "\\\")\", NativeField = true, Global = true)]");
				WriteLine("public static " + i.ClassName + " " + i.PropertyName + " { get { return null; } }");
			}
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				foreach (var i in sel.SubID)
				{
					WriteLine("[JsProperty(Name=\"document.getElementById(\\\"" + ob.ObfuscateID(i.Selector) + "\\\")\", NativeField = true, Global = true)]");
					WriteLine("public static " + i.ClassName + " " + i.PropertyName + " { get { return null; } }");
				}
				WriteLine("#endif");
			}
			EndBracket();
		}

		void RenderSubClass(SelectorData sel)
		{
			if (sel.SubClass.Count == 0)
				return;

			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class Class");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");
			foreach (var i in sel.SubClass)
				WriteLine("public const string " + i.PropertyName + " = \"" + i.Selector + "\";");
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				foreach (var i in sel.SubClass)
					WriteLine("public const string " + i.PropertyName + " = \"" + ob.ObfuscateClass(i.Selector) + "\";");
				WriteLine("#endif");
			}
			EndBracket();
		}

		void RenderSubSelector(SelectorData sel, string selector, string obSelector)
		{
			if (sel.SubClass.Count + sel.SubID.Count == 0)
				return;

			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public static class Selector");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");

			foreach (var i in sel.SubID)
				WriteLine("public const string " + i.PropertyName + " = \"#" + i.Selector + "\";");
			foreach (var i in sel.SubClass)
				WriteLine("public const string " + i.PropertyName + " = \"" + (selector + " ." + i.Selector).Trim() + "\";");

			if (options.MinimizeNames)
			{
				WriteLine("#else");

				foreach (var i in sel.SubID)
					WriteLine("public const string " + i.PropertyName + " = \"#" + ob.ObfuscateID(i.Selector) + "\";");
				foreach (var i in sel.SubClass)
					WriteLine("public const string " + i.PropertyName + " = \"" + (obSelector + " ." + ob.ObfuscateClass(i.Selector)).Trim() + "\";");

				WriteLine("#endif");
			}
			EndBracket();
		}

		void RenderIdSelectors(SelectorData sel, bool obfuscate)
		{
			if (sel.SubID.Count + sel.SubClass.Count == 0)
				return;

			//Sub element properties
			WriteLine("[JsProperty(Name=\"\", NativeField = true)]");
			WriteLine("public ElementsClass By { get; set; }");
			WriteLine("[JsType(JsMode.Json)]");
			Bracket("public class ElementsClass");
			foreach (var sub in sel.SubID)
			{
				string id = (obfuscate ? ob.ObfuscateID(sub.Selector) : sub.Selector);
				WriteLine("[JsProperty(Name=\"querySelector(\\\"#" + id + "\\\")\", NativeField = true)]");
				WriteLine("public " + sub.ClassName + " " + sub.PropertyName + " { get; set; }");
			}
			foreach (var sub in sel.SubClass)
			{
				string classname = (obfuscate ? ob.ObfuscateClass(sub.Selector) : sub.Selector);
				WriteLine("[JsProperty(Name=\"getElementsByClassName(\\\"" + classname + "\\\")[0]\", NativeField = true)]");
				WriteLine("public " + sub.ClassName + " " + sub.PropertyName + " { get; set; }");
			}
			EndBracket();
		}

		public void WriteClasses(List<string> classes, Options options)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket((options.AccessInternal ? "internal" : "public") + " static class Classes");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");

			foreach (string c in classes)
			{
				string cc = Name.ToCamelCase(c);
				WriteLine("public const string " + cc + " = \"" + c + "\";");
			}
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				foreach (string c in classes)
				{
					string cc = Name.ToCamelCase(c);
					string co = ob.ObfuscateClass(c);
					WriteLine("public const string " + cc + " = \"" + co + "\";");
				}
				WriteLine("#endif");
			}
			EndBracket();
		}

		public void WriteSelectors(List<string> classes, Options options)
		{
			WriteLine("[JsType(JsMode.Json)]");
			Bracket((options.AccessInternal ? "internal" : "public") + " static class Selectors");
			if (options.MinimizeNames)
				WriteLine("#if DEBUG");

			foreach (string c in classes)
			{
				string cc = Name.ToCamelCase(c);
				WriteLine("public const string " + cc + " = \"." + c + "\";");
			}
			if (options.MinimizeNames)
			{
				WriteLine("#else");
				foreach (string c in classes)
				{
					string cc = Name.ToCamelCase(c);
					string co = ob.ObfuscateClass(c);
					WriteLine("public const string " + cc + " = \"." + co + "\";");
				}
				WriteLine("#endif");
			}
			EndBracket();
		}
	}
}

