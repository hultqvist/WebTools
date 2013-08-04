using System;
using System.IO;
using System.Text;
using SilentOrbit.Extractor;
using System.Xml;
using System.Collections.Generic;
using SilentOrbit.Parsing;

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
#if DEBUG
			string cwd = Directory.GetCurrentDirectory();
			cwd = Path.GetFullPath(Path.Combine(cwd, "../../../../WebClient/"));
			Directory.SetCurrentDirectory(cwd);
			args = new string[]{
				"--htmlroot", "Html/",
				"--suffix", "Fragment",
				"--namespace", "SilentOrbit.Script",
				"--outputCS", "Script/Generated.cs",
				"--outputHTML", "HtmlRelease/"
			};
#endif
			var options = Options.Parse(args);
			if(options == null)
				return -1;

			try
			{
				using (var output = new Saver(options, ob))
				{
					ScanDir(options, options.WebRoot, output);

					output.WriteClasses(classes);
				}
				return 0;
			}
			catch (XmlException xe)
			{
				Console.Error.WriteLine(xe.Message);
				return -1;
			}
		}

		static void ScanDir(Options options, string path, Saver output)
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

				//Parse HTML ID and class and write modified html at the same time
				string outputPath = Path.Combine(options.OutputHTML, f.Substring(options.WebRoot.Length + 1));
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

				HtmlData data;
				using (FileStream input = File.Open(f, FileMode.Open))
				using (var writer = new HtmlWriter(outputPath))
				{
					ITagOutput tagout = writer;
					tagout = new HtmlCompressor(tagout);
					tagout = new HtmlObfuscator(ob, tagout);
					var extract = new HtmlClassIdExtractor(tagout, f);
					TagParser.Parse(input, extract);

					data = extract.HtmlData;
				}

				//Get IDs and Classes
				//Extract global classes
				data.GetClasses(classes);
				data.GetIDs(ids);

				//Determine Namespace
				string dir = Path.GetDirectoryName(f);
				string ns = dir.Substring(options.WebRoot.Length);
				ns = ns.Replace("/", ".").Trim('.');

				if (ns != "" && data.ClassName == "Index")
				{
					//Move one namespace to classname
					string[] parts = ns.Split('.');
					ns = string.Join(".", parts, 0, parts.Length - 1);

					data.ClassName = parts[parts.Length - 1];
				}

				//Prepare 
				SelectionMerger.Merge(data);

				data.ClassName += options.FileSuffix;

				//Save
				output.WriteClass(ns, data);
				output.Flush();
				Console.WriteLine("Written " + data.ClassName);
			}

			string[] dirs = Directory.GetDirectories(path);
			foreach (string d in dirs)
			{
				ScanDir(options, d, output);
			}
		}
	}
}
