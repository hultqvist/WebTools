using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using SilentOrbit.Code;
using SilentOrbit.Parsing;
using SilentOrbit.Css;
using SilentOrbit.Data;
using SilentOrbit.Extractor;

namespace SilentOrbit
{
	/// <summary>
	/// Scan and extract id and classes from all html files in given directory
	/// </summary>
	class MainClass
	{
		/// <summary>
		/// To generate "Classes" class
		/// </summary>
		static List<string> classes = new List<string>();
		static List<string> ids = new List<string>();
		static Obfuscator ob = new Obfuscator();

		public static int Main(string[] args)
		{
			#if DEBUGx
			args = new string[]{
				"--htmlroot", "TestData",
				"--namespace", "Example",
				"--outputCS", "Generated.cs",
				"--element-property",
				"--generate-classes",
				"--bubble-id",
				"--bubble-class",
				"--outputHTML", "Output",
				"--minimize-names",
				"--inputCSS", Path.Combine("TestData", "style.css"),
				"--outputCSS", Path.Combine("Output", "style.css")
			};
			#endif

			var options = Options.Parse(args);
			if (options == null)
				return -1;

			using (var output = new CSharpSaver(options, ob))
			{
				//Scan HTML
				ScanDir(options, options.WebRoot, output);

				//CSS obfuscation
				if (options.InputCSS != null)
				{
					CssObfuscator.Obfuscate(options.InputCSS, options.OutputCSS, ob);
					Console.WriteLine("Obfuscated written to: " + options.OutputCSS);
				}

				if (options.GenerateGlobalClasses)
				{
					ob.ExportClasses(classes);
					output.WriteClasses(classes, options);
					output.WriteSelectors(classes, options);
				}

				Console.WriteLine("Written: " + options.OutputCS);
			}

			//Export obfuscation data
			using (TextWriter tw = new StreamWriter(Path.Combine(options.WebRoot, "obfuscation.txt")))
			{
				tw.Write(ob.Export());
			}

			return 0;
		}

		static void ScanDir(Options options, string path, CSharpSaver output)
		{
			string index = Path.Combine(path, "index.html");
			string[] files;
			if (File.Exists(index))
				files = new string[] { index };
			else
				files = Directory.GetFiles(path, "*.html", SearchOption.TopDirectoryOnly);

			foreach (string f in files)
			{
				Console.WriteLine("Parsing: " + f);

				HtmlData data;

				//Parse HTML ID and class and write minimized/obfuscated html at the same time
				string outputPath = null;
				if (options.OutputHTML != null)
				{
					outputPath = Path.Combine(options.OutputHTML, f.Substring(options.WebRoot.Length + 1));
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

					using (FileStream input = File.Open(f, FileMode.Open))
					using (var writer = new HtmlFileWriter(outputPath))
					{
						ITagOutput tagout = writer;
						tagout = new HtmlCompressor(tagout);
						tagout = new HtmlObfuscator(ob, tagout);
						var extract = new HtmlClassIdExtractor(tagout, f);
						TagParser.Parse(input, extract);

						data = extract.HtmlData;
					}
				}
				else
				{
					using (FileStream input = File.Open(f, FileMode.Open))
					{
						var hob = new HtmlObfuscator(ob, new NullOutput());
						var extract = new HtmlClassIdExtractor(hob, f);
						TagParser.Parse(input, extract);

						data = extract.HtmlData;
					}
				}

				//Get IDs and Classes
				//Extract global classes
				data.GetClasses(classes);
				data.GetIDs(ids);

				//Determine Namespace
				string dir = Path.GetDirectoryName(f);
				string ns = dir.Substring(options.WebRoot.Length);
				ns = ns.Replace("/", ".").Trim('.');

				if (ns != "" && data.FragmentName == "Index")
				{
					//Move one namespace to classname
					string[] parts = ns.Split('.');
					ns = string.Join(".", parts, 0, parts.Length - 1);

					data.FragmentName = parts[parts.Length - 1];
				}

				//Prepare
				SelectorBubbler.Bubble(data, options);

				data.FragmentName += options.FileSuffix;

				//Save
				output.WriteFragment(ns, data);
			}

			string[] dirs = Directory.GetDirectories(path);
			foreach (string d in dirs)
			{
				ScanDir(options, d, output);
			}
		}
	}
}
