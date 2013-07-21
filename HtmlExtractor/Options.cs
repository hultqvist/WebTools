using System;
using CommandLine;
using CommandLine.Text;
using System.IO;
using System.Linq;

namespace SilentOrbit
{
	class Options
	{
		/// <summary>
		/// Root path to the html files.
		/// </summary>
		[Option("htmlroot", Required = true, HelpText = "Path to the html directory, subdirectories will be included")]
		public string WebRoot { get; set; }

		[Option("namespace", Required = true, HelpText = "Root Namespace")]
		public string Namespace { get; set; }

		[Option("output", Required = true, HelpText = "Output path to generated .cs file")]
		public string OutputCS { get; set; }

		[Option("suffix", Required = false, HelpText = "Append to the end of each file related class name")]
		public string FileSuffix { get; set; }

		public static Options Parse(string[] args)
		{
			var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
			var options = result.Value;
			if (result.Errors.Any())
			{
				//Console.WriteLine(HelpText.AutoBuild<Options>(result));
				return null;
			}

			options.WebRoot = Path.GetFullPath(options.WebRoot).TrimEnd(Path.DirectorySeparatorChar);
			options.OutputCS = Path.GetFullPath(options.OutputCS);
			return options;
		}
	}
}

