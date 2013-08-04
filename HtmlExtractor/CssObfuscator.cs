using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
			int p = 0;
			while (true)
			{
				int start = css.IndexOf("{", p);
				if (start < 0)
					break;
				int end = css.IndexOf("}", start);

				//Posible ',' spearated multiple selector groups
				string sel = css.Substring(p, start - p).Trim(wsp);
				string[] selGroup = sel.Split(',');

				for (int n = 0; n < selGroup.Length; n++)
					selGroup[n] = ObfuscateSelectors(selGroup[n]);

				//Write selectors
				sel = string.Join(",", selGroup);
				writer.WriteLine(sel);

				//Content inside {}
				writer.WriteLine(css.Substring(start, end - start + 1));

				p = end + 1;
			}
			writer.Write(css.Substring(p));
		}

		string ObfuscateSelectors(string selector)
		{
			selector = selector.Trim(wsp);

			//Obfuscate selector
			string[] sel = selector.Split(' ', '>', '*');
			for (int n = 0; n < sel.Length; n++)
			{
				string s = sel[n].Trim(wsp);
				int colon = s.IndexOf(':');
				if (colon > 0)
					s = s.Substring(0, colon);

				if (s.StartsWith("#"))
				{
					var o = ob.ObfuscateID(s.Substring(1));
					if (o == null)
						Console.Error.WriteLine("Warning, css id not used in html: " + s);
					else
						sel[n] = "#" + o;
					continue;
				}
				if (s.StartsWith("."))
				{
					var o = ob.ObfuscateClass(s.Substring(1));
					if (o == null)
						Console.Error.WriteLine("Warning, css class not used in html: " + s);
					else
						sel[n] = "." + o;
					continue;
				}
				if (s.StartsWith("@"))
					break;
			}
			return string.Join(" ", sel);
		}
	}
}

