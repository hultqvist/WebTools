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
			if (args.Length != 3)
			{
				Console.Error.WriteLine("Usage: HtmlExtractor.exe WebRootPath/ Generated.Namespace Output.cs");
				return -1;
			}

			string webRoot = Path.GetFullPath(args[0]).TrimEnd(Path.DirectorySeparatorChar);
			string rootNamespace = args[1];
			string outputPath = Path.GetFullPath(args[2]);

			try
			{
				using (var output = new Saver(outputPath, rootNamespace))
				{
					ScanDir(webRoot, webRoot, output);

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

		static void ScanDir(string webRoot, string path, Saver output)
		{
			string real = Path.Combine(path, "real");
			if (File.Exists(real))
			{
				string rc = File.ReadAllText(real).Trim(' ', '\r', '\n');
				string file = Path.GetFileName(path);
				if (file != rc)
					return;
			}

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
				string ns = dir.Substring(webRoot.Length);
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

				//Save
				output.WriteClass(ns, data);
				output.Flush();
				Console.WriteLine("Written " + data.ClassName);
			}

			string[] dirs = Directory.GetDirectories(path);
			foreach (string d in dirs)
			{
				ScanDir(webRoot, d, output);
			}
		}
	}
}
