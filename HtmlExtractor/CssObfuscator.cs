using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SilentOrbit
{
	public class CssObfuscator
	{
		readonly string css;
		readonly TextWriter writer;
		readonly Obfuscator ob;

		CssObfuscator(string css, TextWriter writer, Obfuscator ob)
		{
			this.css = css;
			this.writer = writer;
			this.ob = ob;
		}

		public static void Obfuscate(string input, string output, Obfuscator ob)
		{
			using (TextReader reader = new StreamReader(input, Encoding.UTF8))
			{
				TextWriter writer = null;
				if (output == null)
					writer = new StringWriter();
				else
					writer = new StreamWriter(output, false, Encoding.UTF8);

				var co = new CssObfuscator(
					         reader.ReadToEnd(),
					         writer,
					         ob);
				co.Obfuscate();

				writer.Close();
			}
		}

		static char[] wsp = new char[] { ' ', '\t', '\r', '\n' };

		void Obfuscate()
		{
			Obfuscate(css);
		}

		void Obfuscate(string unparsed)
		{
			unparsed = unparsed.Trim(wsp);
			while (true)
			{
				if (unparsed == "")
					break;
				
				if (unparsed.StartsWith("/*"))
				{
					int end = unparsed.IndexOf("*/");
					if (end < 0)
						break;
					unparsed = unparsed.Substring(end + 2).Trim(wsp);
					continue;
				}
				
				if (unparsed.StartsWith("@import"))
				{
					int end = unparsed.IndexOfAny(new char[] { ';', '{', '}' });
					if (end < 0 || unparsed[end] != ';')
						throw new InvalidDataException("Missing ; after @import");
						
					writer.WriteLine(unparsed.Substring(0, end + 1));
					unparsed = unparsed.Substring(end + 1).Trim(wsp);
					continue;
				}
				
				if (unparsed.StartsWith("@media"))
				{
					int end = unparsed.IndexOfAny(new char[] { ';', '{', '}' });
					if (end < 0 || unparsed[end] != '{')
						throw new InvalidDataException("Missing { after @media");
					
					writer.WriteLine(unparsed.Substring(0, end + 1));
					unparsed = unparsed.Substring(end + 1).Trim(wsp);
					
					//Find ending "}" count brackets only
					//We assume "{" and "}" only exist as block separators and not inside strings.
					int nesting = 1;
					end = 0;
					while (true)
					{
						end = unparsed.IndexOfAny(new char[] { '{', '}' }, end);
						if (end < 0)
							throw new InvalidDataException("Unmatched brackets");
						if (unparsed[end] == '{')
						{
							nesting += 1;
							end += 1;
							continue;
						}
						if (unparsed[end] == '}')
						{
							nesting -= 1;
							if (nesting == 0)
								break; //end points at ending "}"
							end += 1;
							continue;
						}
						throw new InvalidProgramException();
					}
					
					string inside = unparsed.Substring(0, end);
					Obfuscate(inside);
					
					writer.WriteLine("}");
					unparsed = unparsed.Substring(end + 1).Trim(wsp);
					continue;
				}
				
				//css selector rule
				{
					int end = unparsed.IndexOfAny(new char[] { ';', '{', '}' });
					if (end < 0 || unparsed[end] != '{')
						throw new InvalidDataException("Missing { after css selector");
					string sel = unparsed.Substring(0, end);
					unparsed = unparsed.Substring(end + 1).Trim(wsp);
					
					//Possible ',' spearated multiple selector groups
					string[] selGroup = sel.Split(',');
	
					for (int n = 0; n < selGroup.Length; n++)
						selGroup[n] = ObfuscateSelectors(selGroup[n]);

					//Write selectors
					sel = string.Join(",", selGroup);
					writer.Write(sel + "{");

					//Content inside {}
					end = unparsed.IndexOfAny(new char[] { '{', '}' });
					if (end < 0 || unparsed[end] != '}')
						throw new InvalidDataException("Missing }");
					writer.WriteLine(unparsed.Substring(0, end + 1)); //Include "}"
					
					unparsed = unparsed.Substring(end + 1).Trim(wsp);
				}
			}
		}

		string ObfuscateSelectors(string selector)
		{
			selector = selector.Trim(wsp);

			//Obfuscate selector
			var sel = new List<string>(selector.Split(' ', '>', '*'));
			for (int n = 0; n < sel.Count; n++)
			{
				string s = sel[n].Trim(wsp);
				int colon = s.IndexOf(':');
				if (colon > 0)
					s = s.Substring(0, colon);
				//Detect ".foo.bar" and split, doesn't work for .foo.bar.baz
				int pos = s.LastIndexOf('.');
				if (pos > 0)
				{
					//TODO: split on '.' and obfuscate each part
					string first = s.Substring(0, pos);
					string second = s.Substring(pos);
					sel[n] = ObfuscateSelectorPart(first) + ObfuscateSelectorPart(second);
					continue;
				}

				sel[n] = ObfuscateSelectorPart(s);
				if (s.StartsWith("@"))
					break;
			}
			return string.Join(" ", sel);
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
			return s;
		}
	}
}

