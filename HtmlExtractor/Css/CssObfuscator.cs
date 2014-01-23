using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SilentOrbit.Data;
using ExCSS;

namespace SilentOrbit.Css
{
	public class CssObfuscator
	{
		readonly StyleSheet css;
		readonly Obfuscator ob;

		CssObfuscator(StyleSheet css, Obfuscator ob)
		{
			this.css = css;
			this.ob = ob;
		}

		public static void Obfuscate(string input, string output, Obfuscator ob)
		{
			var p = new ExCSS.Parser();
			var stylesheet = p.Parse(File.ReadAllText(input, Encoding.UTF8));
			var co = new CssObfuscator(stylesheet, ob);
			co.Obfuscate();

			if (output != null)
			{
				string css = stylesheet.ToString();
				File.WriteAllText(output, css, Encoding.UTF8);
			}
		}

		//static char[] wsp = new char[] { ' ', '\t', '\r', '\n' };

		void Obfuscate()
		{
			foreach (MediaRule m in css.MediaDirectives)
			{
				foreach (StyleRule r in m.Declarations)
					ObfuscateSelectors(r);
			}

			foreach (RuleSet rs in css.Rulesets)
			{
				var r = rs as StyleRule;
				if (r == null)
					continue;
				ObfuscateSelectors(r);
			}
		}

		void ObfuscateSelectors(StyleRule rule)
		{
			rule.Selector = ObfuscateSelectors(rule.Selector);
		}

		SimpleSelector ObfuscateSelectors(SimpleSelector selector)
		{
			Type ruleType = selector.GetType();

			if (ruleType == typeof(SimpleSelector))
			{
				string obf = ObfuscateSelectorPart(selector.ToString());
				if (obf == null)
					return selector;
				else
					return new SimpleSelector(obf);
			}

			if (ruleType == typeof(ComplexSelector))
			{
				var obf = new ComplexSelector();
				var cs = (ComplexSelector)selector;
				foreach (var sel in cs)
					obf.AppendSelector(ObfuscateSelectors(sel.Selector), sel.Delimiter);
				return obf;
			}

			if (ruleType == typeof(AggregateSelectorList))
			{
				var obf = new AggregateSelectorList();
				var cs = (AggregateSelectorList)selector;
				foreach (SimpleSelector sel in cs)
					obf.AppendSelector(ObfuscateSelectors(sel));
				return obf;
			}

			if (ruleType == typeof(MultipleSelectorList))
			{
				var obf = new MultipleSelectorList();
				var cs = (MultipleSelectorList)selector;
				foreach (SimpleSelector sel in cs)
					obf.AppendSelector(ObfuscateSelectors(sel));
				return obf;
			}

			throw new NotImplementedException("Type: " + ruleType);
		}

		string ObfuscateSelectorPart(string s)
		{
			if (s.StartsWith("#"))
			{
				var o = ob.ObfuscateID(s.Substring(1));
				if (o == null)
					Console.Error.WriteLine("Warning, css id not used in html: " + s);
				else
					return "#" + o;
			}
			if (s.StartsWith("."))
			{
				var o = ob.ObfuscateClass(s.Substring(1));
				if (o == null)
					Console.Error.WriteLine("Warning, css class not used in html: " + s);
				else
					return "." + o;
			}
			return null;
		}
	}
}

