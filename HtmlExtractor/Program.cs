using System;
using System.IO;
using System.Text;
using SilentOrbit.Extractor;
using System.Xml;
using System.Collections.Generic;

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

		public static int Main(string[] args)
		{
			var options = Options.Parse(args);
			if(options == null)
				return -1;

			try
			{
				using (var output = new Saver(options))
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
				//Get IDs and Classes
				var data = HtmlParser.Extract(f);

				//Extract global classes
				data.GetClasses(classes);
				data.GetIDs(classes);

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
